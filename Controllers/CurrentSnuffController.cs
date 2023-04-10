using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class CurrentSnuffController : ControllerBase
{
    private readonly ILogger<CurrentSnuffController> _logger;

    public CurrentSnuffController(ILogger<CurrentSnuffController> logger)
    {
        _logger = logger;
    }

    // [HttpGet("CurrentSnuff")]
    // {
         
    // }
   
}