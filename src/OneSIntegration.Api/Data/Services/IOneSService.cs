namespace OneSIntegration.Api.Data.Services; 
 
public interface IOneSService 
{ 
    Task<string> GetCounterpartiesAsync(); 
    Task<string> GetNomenclatureAsync(); 
} 
