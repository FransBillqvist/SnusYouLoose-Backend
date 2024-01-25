using DAL;
using DAL.Dto;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetDto/{userid}")]
    public async Task<ActionResult<HabitDto>> GetHabitDto(string userid)
    {
        try
        {
            var habit = await _habitService.GetHabitDtoAsync(userid);
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

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("CreateV2/{userid}")]

    public async Task<ActionResult<HabitDto>> CreateHabit([FromBody] HabitDto habit, string userid)
    {
        Console.WriteLine("Hello, Am CreateHabit Endpoint, with createdatutc time: " + DateTime.Now);
        try
        {
            var response = await _habitService.CreateHabitFromRequestAsync(habit, userid);
            return response;
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("Create")]

    public async Task<ActionResult<Habit>> Post([FromBody] Habit newHabit)
    {
        Console.WriteLine("Hello, Am Post Habit Endpoint, with createdatutc time: " + DateTime.Now);
        try
        {
            newHabit.CreatedAtUtc = DateTime.UtcNow;
            newHabit.StartDate = DateTime.UtcNow;
            newHabit.EndDate = DateTime.UtcNow;
            await _habitService.CreateHabitAsync(newHabit);
            return newHabit;
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
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
