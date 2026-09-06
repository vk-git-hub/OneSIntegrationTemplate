using OneSIntegration.Api.Models; 
 
public class IntegrationLog 
{ 
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Endpoint { get; set; } = string.Empty;

    public string Request { get; set; } = string.Empty;
    
    // JSON
    public string Response { get; set; } = string.Empty;

    // JSON
    public int StatusCode { get; set; }     

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
} 
