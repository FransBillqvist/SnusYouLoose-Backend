using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitController : ControllerBase
{
    private readonly ILogger<HabitController> _logger;
    private readonly IGenericMongoRepository<Habit> _habitRepository;
    public HabitController(ILogger<HabitController> logger, IGenericMongoRepository<Habit> habitRepository)
    {
        _logger = logger;
        _habitRepository = habitRepository;
    }

    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<Habit>> GetHabit(string id)
    {
        try
        {
            var habit = await _habitRepository.FindOneAsync(x => x.Id == id);

            if (habit is null)
            {
                return null;
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
    public async Task<IActionResult> Post(Habit newHabit)
    {
        try
        {
            await _habitRepository.InsertOneAsync(newHabit);
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
            var habit = await _habitRepository.FindByIdAsync(id);

            if (habit is null)
            {
                return NotFound();
            }


            updatedHabit.Id = habit.Id;

            await _habitRepository.ReplaceOneAsync(updatedHabit);

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
            var habit = await _habitRepository.FindByIdAsync(id);

            if (habit is null)
            {
                return NotFound();
            }

            await _habitRepository.DeleteByIdAsync(id);

            return NoContent();
        }
        catch
        {
            return NotFound();
        }
    }
}
