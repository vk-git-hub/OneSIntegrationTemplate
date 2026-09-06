using Microsoft.Extensions.Options;
using OneSIntegration.Api.Data;
using OneSIntegration.Api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OneSIntegration.Api.Data.Services;

public class OneSOptions
{
    public string BaseUrl { get; set; } = string.Empty; 
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


public class OneSODataService : IOneSService
{
    private readonly HttpClient _http; private readonly OneSOptions _options; private readonly AppDbContext _db;
    private readonly ILogger<OneSODataService> _logger;

    public OneSODataService(HttpClient http, IOptionsMonitor<OneSOptions> options, AppDbContext db, ILogger<OneSODataService> logger)
    {
        _http = http;
        _options = options.CurrentValue;
        _db = db;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw new InvalidOperationException("OneS:BaseUrl не задан в appsettings.json");
        if (string.IsNullOrWhiteSpace(_options.UserName))
            throw new InvalidOperationException("OneS:UserName не задан");

        // Нормализуем BaseUrl: если уже содержит odata — не дублируем
        var baseUrl = _options.BaseUrl.Trim().TrimEnd('/');
        if (!baseUrl.Contains("odata", StringComparison.OrdinalIgnoreCase))
            baseUrl += "/odata/standard.odata";
        baseUrl = baseUrl.TrimEnd('/') + "/";

        _http.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        // Важно: НЕ ставим Authorization в DefaultRequestHeaders — HttpClientFactory пулит handler и при редиректе .NET сбрасывает этот заголовок.
        // Ставим его на каждый HttpRequestMessage отдельно (см. ExecuteRequestAsync).
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.Timeout = TimeSpan.FromSeconds(30);

        _logger.LogInformation("1C OData BaseAddress: {BaseAddress}, User: {User}", _http.BaseAddress, _options.UserName);
    }

    public async Task<string> GetCounterpartiesAsync()
    {
        return await ExecuteRequestAsync("Catalog_Контрагенты");
    }

    public async Task<string> GetNomenclatureAsync()
    {
        return await ExecuteRequestAsync("Catalog_Номенклатура");
    }

    private async Task<string> ExecuteRequestAsync(string entitySet)
    {
        // Кириллицу в имени Catalog_Контрагенты нужно percent-encode
        var encodedEntity = Uri.EscapeDataString(entitySet);
        var requestUrl = $"{encodedEntity}?$format=json";
        var fullUrl = new Uri(_http.BaseAddress!, requestUrl);
        _logger.LogInformation("1C OData GET {Url}", fullUrl);

        // HttpClient.GetAsync никогда не возвращает null по контракту — если видите null в отладчике, значит await не завершился (deadlock из-за .Result/.Wait()) или исключение до присвоения.
        // Отправляем через HttpRequestMessage с явным Authorization на каждый запрос (аналог curl -u), чтобы редирект не сбросил заголовок.
        var encoding = Encoding.GetEncoding("iso-8859-1");
        var auth = Convert.ToBase64String(encoding.GetBytes($"{_options.UserName}:{_options.Password}"));
        using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _http.SendAsync(request);
        if (response == null)
            throw new InvalidOperationException("HttpClient.SendAsync вернул null — проверьте кастомный HttpMessageHandler / IHttpClientFactory");
        var content = await response.Content.ReadAsStringAsync();

        // Диагностика HTML-ответа от портала 1cFRESH (когда BaseUrl неверный — приходит страница 1cfresh.com вместо JSON)
        var isHtml = content.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
                  || content.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase);

        // Логируем в БД 
        _db.IntegrationLogs.Add(new IntegrationLog
        {
            Endpoint = entitySet,
            Request = fullUrl.ToString(),
            Response = isHtml ? content.Substring(0, Math.Min(content.Length, 4000)) : content,
            StatusCode = (int)response.StatusCode
        });
        await _db.SaveChangesAsync();

        if (isHtml)
        {
            throw new HttpRequestException(
                $"1C API вернул HTML вместо JSON (обычно неверный BaseUrl или логин НЕ из информационной базы). " +
                $"URL: {fullUrl}, Status: {response.StatusCode}. " +
                $"Проверьте: 1) BaseUrl должен указывать на публикацию ИБ, напр. https://msk1.1cfresh.com/a/sbm/4191671/ru/odata/standard.odata/ " +
                $"2) UserName/Password — это пользователь ИБ (созданный в Конфигураторе/1С, не кабинет 1cFresh!), " +
                $"3) Включен OData в «Администрирование → Публикация OData». Тело HTML обрезано: {content.Substring(0, Math.Min(500, content.Length))}");
        }

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"1C API error: {response.StatusCode}, body: {content}");

        return content;
    }
}
