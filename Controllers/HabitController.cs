using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitController : ControllerBase
{
    private readonly ILogger<HabitController> _logger;
    private readonly IGenericMongoRepository<Habit> _habitRepository;
    private readonly IHabitService _habitService;
    public HabitController(
        ILogger<HabitController> logger,
        IGenericMongoRepository<Habit> habitRepository,
        IHabitService habitService)
    {
        _logger = logger;
        _habitRepository = habitRepository;
        _habitService = habitService;
    }

    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<Habit>> GetHabit(string id)
    {
        try
        {
            var habit = await _habitService.GetHabitAsync(id);
            if (habit is null)
            {
                return NotFound();
            }

            return habit;
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Route("Create")]

    public async Task<IActionResult> Post([FromBody] Habit newHabit)
    {
        Console.WriteLine("Hello, Am Post Habit Endpoint");
        try
        {
            newHabit.CreatedAtUtc = DateTime.UtcNow;
            newHabit.StartDate = DateTime.UtcNow;
            newHabit.EndDate = DateTime.UtcNow;
            await _habitService.CreateHabitAsync(newHabit);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, Habit updatedHabit)
    {
        try
        {
            var habit = await _habitService.GetHabitAsync(id);

            if (habit is null)
            {
                return NotFound();
            }

            await _habitService.UpdateHabitAsync(id, updatedHabit);

            return Ok();
        }
        catch
        {
            return NoContent();
        }
    }

    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var habit = await _habitService.GetHabitAsync(id);

            if (habit is null)
            {
                return NotFound();
            }

            await _habitService.RemoveHabitAsync(id);

            return NoContent();
        }
        catch
        {
            return NotFound();
        }
    }
}
