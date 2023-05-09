using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnuffController : ControllerBase
{
    private readonly ILogger<SnuffController> _logger;
    private readonly IGenericMongoRepository<Snuff> _snuffRepository;
    private readonly ISnuffService _snuffService;

    public SnuffController(
            ILogger<SnuffController> logger,
            IGenericMongoRepository<Snuff> snuffRepository,
            ISnuffService snuffService)
    {
        _logger = logger;
        _snuffRepository = snuffRepository;
        _snuffService = snuffService;
    }

    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<Snuff>> GetSnuff(string id)
    {
        try
        {
            var response = await _snuffService.GetSnuffAsync(id);

            if (response is null)
            {
                return null;
            }

            return response;
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(Snuff newSnuff)
    {
        try
        {
            await _snuffService.CreateSnuffAsync(newSnuff);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, Snuff updatedSnuff)
    {
        try
        {
            var snuffToUpdate = await _snuffService.GetSnuffAsync(id);
            if (snuffToUpdate is null)
            {
                return NotFound();
            }

            await _snuffService.UpdateSnuffAsync(id, updatedSnuff);
            var checkIfUpdateWasOK = await _snuffService.GetSnuffAsync(id);

            return Ok(checkIfUpdateWasOK);
        }

        catch
        {
            return BadRequest();
        }
    }

    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var snuffToDelete = await _snuffService.GetSnuffAsync(id);
        if (snuffToDelete is null)
        {
            return NotFound();
        }

        await _snuffService.RemoveSnuffAsync(id);

        return NoContent();
    }
}
