using Microsoft.AspNetCore.Mvc;
using OneSIntegration.Api.Data.Services;

[ApiController] 
[Route("api/[controller]")] public class NomenclatureController : ControllerBase 
{ 
    private readonly IOneSService _oneS; 
 
    public NomenclatureController(IOneSService oneS) => _oneS = oneS; 
 
    [HttpGet]     public async Task<IActionResult> GetAll() 
    { 
        var rawJson = await _oneS.GetNomenclatureAsync();
        return Content(rawJson, "application/json"); 
    } 
} 
