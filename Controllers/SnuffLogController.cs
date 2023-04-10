using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class SnuffLogController : ControllerBase
{
    private readonly ILogger<SnuffLogController> _logger;

    public SnuffLogController(ILogger<SnuffLogController> logger)
    {
        _logger = logger;
    }

    // [HttpGet("SnuffLog")]
    // {
        
    // }
   
}
