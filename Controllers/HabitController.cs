using DAL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class HabitController : ControllerBase
{
    private readonly ILogger<HabitController> _logger;
    private readonly HabitService _habitService;
    public HabitController(ILogger<HabitController> logger, HabitService habitService)
    {
        _logger = logger;
        _habitService = habitService;
    }

    [HttpGet]
    public async Task<List<Habit>> Get() => await _habitService.GetAllHabitsAsync();

    [HttpGet]
    public async Task<ActionResult<Habit>> Get(string id)
    {
        var habit = await _habitService.GetHabitAsync(id);

        if (habit is null)
        {
            return NotFound();
        }

        return habit;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Habit newHabit)
    {
        await _habitService.CreateHabitAsync(newHabit);

        return CreatedAtAction(nameof(Get), new { id = newHabit.Id }, newHabit);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, Habit updatedHabit)
    {
        var habit = await _habitService.GetHabitAsync(id);

        if (habit is null)
        {
            return NotFound();
        }

        updatedHabit.Id = habit.Id;

        await _habitService.UpdateHabitAsync(id, updatedHabit);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var habit = await _habitService.GetHabitAsync(id);

        if (habit is null)
        {
            return NotFound();
        }

        await _habitService.RemoveHabitAsync(id);

        return NoContent();
    }
   
}
