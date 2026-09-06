using Microsoft.EntityFrameworkCore;
using OneSIntegration.Api.Data;
using OneSIntegration.Api.Data.Services;

var builder = WebApplication.CreateBuilder(args); 
 
builder.Services.AddControllers(); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(); 
 
// PostgreSQL + EF Core
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); 
 
// Регистрация 1С‑сервиса
builder.Services.Configure<OneSOptions>(builder.Configuration.GetSection("OneS"));
builder.Services.AddHttpClient<IOneSService, OneSODataService>(client =>
{
    // curl по умолчанию шлёт User-Agent и Accept, 1cFRESH WAF иногда режет запросы без них
    client.DefaultRequestHeaders.UserAgent.ParseAdd("OneSIntegration/1.0 (+https://1cfresh.com)");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // 1cFRESH делает 302 на кластер (msk1 -> ...), .NET по умолчанию сбрасывает Authorization при редиректе.
    // Если curl -v показывает 302, нужно либо запретить авторедирект и обработать вручную, либо убедиться что BaseUrl уже финальный.
    AllowAutoRedirect = true
}); 
 
var app = builder.Build();

// Автоматические миграции (для демо; в продакшене лучше явно)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Если есть миграции — применяем их, иначе создаём базу с нуля
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

//if (app.Environment.IsDevelopment()) 
//{ 
app.UseSwagger();     app.UseSwaggerUI();
//} 

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers(); app.Run(); 
