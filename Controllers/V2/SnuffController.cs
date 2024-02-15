using DAl.Dto;
using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<Snuff>> GetSnuff(string id)
    {
        try
        {
            var response = await _snuffService.GetSnuffAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return response;
        }
        catch
        {
            return NotFound();
        }
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetV2/{id}")]
    public async Task<ActionResult<SnuffShopDto>> GetSnuffV2(string id)
    {
        try
        {
            var response = await _snuffService.GetSnuffViaIdAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return response;
        }
        catch
        {
            return NotFound();
        }
    }

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(Snuff newSnuff)
    {
        try
        {
            Console.WriteLine($"Create Snuff data for Snuff(ENDPOINT): {newSnuff.Type}");
            newSnuff.CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1);
            await _snuffService.CreateSnuffAsyncV2(newSnuff);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetShopSnuffs")]
    /// <summary>
    /// Get all snuffs use case for shopping list, where we need to get all snuffs
    /// </summary>
    public async Task<List<SnuffShopDto>> GetAllSortOfSnuffs()
    {
        var response = await _snuffService.GetShopSnuffsAsync();
        return response;
    }
  
}
