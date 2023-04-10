using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class SnuffController : ControllerBase
{
    private readonly ILogger<SnuffController> _logger;

    public SnuffController(ILogger<SnuffController> logger)
    {
        _logger = logger;
    }

    // [HttpGet("Snuff")]
    // {
        
    // }
   
}
