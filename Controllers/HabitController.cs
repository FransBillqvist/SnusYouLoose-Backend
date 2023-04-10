using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class HabitController : ControllerBase
{
    private readonly ILogger<HabitController> _logger;

    public HabitController(ILogger<HabitController> logger)
    {
        _logger = logger;
    }

    // [HttpGet("Habit")]
    // {
        
    // }
   
}
