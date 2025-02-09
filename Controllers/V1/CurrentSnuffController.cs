using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.Interfaces;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CurrentSnuffController : ControllerBase
{

    private readonly ILogger<CurrentSnuffController> _logger;
    private readonly IGenericMongoRepository<CurrentSnuff> _csRepository;
    private readonly ICurrentSnuffService _csService;
    private readonly ISnuffService _sService;

    public CurrentSnuffController(
        ILogger<CurrentSnuffController> logger,
        IGenericMongoRepository<CurrentSnuff> CSRepository,
        ICurrentSnuffService CSService,
        ISnuffService SService)
    {
        _logger = logger;
        _csRepository = CSRepository;
        _csService = CSService;
        _sService = SService;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [Route("GetAllSnuff/{uid}")]
    public async Task<List<CurrentSnuff>> GetActiveSnuffForUser(string uid)
    {
        var result = await _csService.GetAllCurrentSnuffsForThisUserAsync(uid);
        return result;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<CurrentSnuff>> GetCurrentSnuff(string id)
    {
        var currentSnuff = await _csService.GetCurrentSnuffAsync(id);
        if (currentSnuff is null)
        {
            return NotFound();
        }

        return currentSnuff;
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    [Route("Create")]
    public async Task<ActionResult<CurrentSnuff>> Post(CurrentSnuff newCurrentSnuff)
    {
        try
        {
            newCurrentSnuff.CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1);
            newCurrentSnuff.PurchaseDate = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1);;
            await _csService.CreateCurrentSnuffAsync(newCurrentSnuff);
            return newCurrentSnuff;
        }

        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    [Route("NewSnuffLog")]
    public async Task<IActionResult> AddLog(string id, int amount, string userId)
    {

        try
        {
            await _csService.LogAdder(id, amount, userId);
            return Ok();
        }

        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("1.0")]
    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var currentSnuffToDelete = _csService.GetCurrentSnuffAsync(id);
            if (currentSnuffToDelete is null)
            {
                return NotFound();
            }

            await _csService.RemoveCurrentSnuffAsync(id);
            return NoContent();
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    [Route("AddCurrentSnuffToArchive/{csId}")]
    public async Task<IActionResult> AddCurrentSnuffToArchive(string csId)
    {
        try
        {
            return Ok(await _csService.AddCurrentSnuffToArchiveAsync(csId));
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [Route("GetRemainingSnuffInBox/{csId}")]
    public async Task<int> GetRemainingSnuffInBox(string csId)
    {
        try
        {
            return await _csService.GetAmountInBoxAsync(csId);
        }
        catch
        {
            return 0;
        }
    }
}