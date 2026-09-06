using Microsoft.AspNetCore.Mvc;
using OneSIntegration.Api.Data.Services;

namespace OneSIntegration.Api.Controllers; 
 
[ApiController] 
[Route("api/[controller]")] public class CounterpartiesController : ControllerBase 
{ 
    private readonly IOneSService _oneS; 
 
    public CounterpartiesController(IOneSService oneS) => _oneS = oneS; 
 
    [HttpGet]     public async Task<IActionResult> GetAll() 
    { 
        var rawJson = await _oneS.GetCounterpartiesAsync();
        return Content(rawJson, "application/json"); 
    } 
} 
