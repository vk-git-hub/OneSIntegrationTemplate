using System.Text.Json.Serialization; 
 
namespace OneSIntegration.Api.Models.OneS; 
 
public class Counterparty 
{ 
    [JsonPropertyName("Ref_Key")]
    public string RefKey { get; set; } = string.Empty; 
 
    [JsonPropertyName("Description")]
    public string Name { get; set; } = string.Empty; 
 
    [JsonPropertyName("ИНН")]
    public string Inn { get; set; } = string.Empty; 
} 
