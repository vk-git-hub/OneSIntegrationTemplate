using System.Text.Json.Serialization; 
 
namespace OneSIntegration.Api.Models.OneS; 
 
public class Nomenclature 
{ 
    [JsonPropertyName("Ref_Key")]
    public string RefKey { get; set; } = string.Empty; 
 
    [JsonPropertyName("Description")]
    public string Name { get; set; } = string.Empty; 
 
    [JsonPropertyName("Article")]
    public string Article { get; set; } = string.Empty; 
} 
