using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.Interfaces;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    [Route("GetAllSnuff/{uid}")]
    public async Task<List<CurrentSnuff>> GetActiveSnuffForUser(string uid)
    {
        var result = await _csService.GetAllCurrentSnuffsForThisUserAsync(uid);
        return result;
    }


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

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(CurrentSnuff newCurrentSnuff)
    {
        try
        {
            newCurrentSnuff.CreatedAtUtc = DateTime.UtcNow;
            newCurrentSnuff.PurchaseDate = DateTime.UtcNow;
            await _csService.CreateCurrentSnuffAsync(newCurrentSnuff);
            return Ok();
        }

        catch
        {
            return BadRequest();
        }
    }

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
}