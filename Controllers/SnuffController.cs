using DAL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class SnuffController : ControllerBase
{
    private readonly ILogger<SnuffController> _logger;
    private readonly SnuffService _snuffService;

    public SnuffController(ILogger<SnuffController> logger, SnuffService snuffService)
    {
        _logger = logger;
        _snuffService = snuffService;
    }

    [HttpGet]
    public async Task<List<Snuff>> Get() => await _snuffService.GetAllSnuffAsync();

    [HttpGet]
    public async Task<ActionResult<Snuff>> Get(string id)
    {
        var snuff = await _snuffService.GetSnuffAsync(id);

        if (snuff is null)
        {
            return NotFound();
        }

        return snuff;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Snuff newSnuff)
    {
        await _snuffService.CreateSnuffAsync(newSnuff);

        return CreatedAtAction(nameof(Get), new { id = newSnuff.Id }, newSnuff);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, Snuff updatedSnuff)
    {
        var snuff = await _snuffService.GetSnuffAsync(id);

        if (snuff is null)
        {
            return NotFound();
        }

        updatedSnuff.Id = snuff.Id;

        await _snuffService.UpdateSnuffAsync(id, updatedSnuff);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var snuff = await _snuffService.GetSnuffAsync(id);

        if (snuff is null)
        {
            return NotFound();
        }

        await _snuffService.RemoveSnuffAsync(id);

        return NoContent();
    }
   
}
