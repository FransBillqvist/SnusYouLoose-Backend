using Services;
using Microsoft.AspNetCore.Mvc;
using DAL;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrentSnuffController : ControllerBase
{
    private readonly ILogger<CurrentSnuffController> _logger;
    private readonly CurrentSnuffService _currentSnuffService;

    public CurrentSnuffController(ILogger<CurrentSnuffController> logger, CurrentSnuffService currentSnuffService)
    {
        _logger = logger;
        _currentSnuffService = currentSnuffService;
    }

    [HttpGet]
    public async Task<List<CurrentSnuff>> Get() => await _currentSnuffService.GetAllCurrentSnuffAsync();

    [HttpGet]
    public async Task<ActionResult<CurrentSnuff>> Get(string id)
    {
        var currentSnuff = await _currentSnuffService.GetCurrentSnuffAsync(id);

        if (currentSnuff is null)
        {
            return NotFound();
        }

        return currentSnuff;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CurrentSnuff newCurrentSnuff)
    {
        await _currentSnuffService.CreateCurrentSnuffAsync(newCurrentSnuff);

        return CreatedAtAction(nameof(Get), new { id = newCurrentSnuff.Id }, newCurrentSnuff);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, CurrentSnuff updatedCurrentSnuff)
    {
        var currentSnuff = await _currentSnuffService.GetCurrentSnuffAsync(id);

        if (currentSnuff is null)
        {
            return NotFound();
        }

       updatedCurrentSnuff.Id = currentSnuff.Id;

        await _currentSnuffService.UpdateCurrentSnuffAsync(id, updatedCurrentSnuff);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var currentSnuff = await _currentSnuffService.GetCurrentSnuffAsync(id);

        if (currentSnuff is null)
        {
            return NotFound();
        }

        await _currentSnuffService.RemoveCurrentSnuffAsync(id);

        return NoContent();
    }
}