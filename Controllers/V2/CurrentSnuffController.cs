using DAL;
using DAL.Dto;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]

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
    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetAllSnuff/{uid}")]
    public async Task<List<CurrentSnuff>> GetActiveSnuffForUser(string uid)
    {
        var result = await _csService.GetAllCurrentSnuffsForThisUserAsync(uid);
        Console.WriteLine($"userid is: {uid}");
        foreach (var item in result)
        {
            Console.WriteLine(item.SnusId);
        }
        Console.WriteLine("----------GetActiveSnuffForUser----------------");
        return result;
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<CurrentSnuff>> GetCurrentSnuff(string id)
    {
        var currentSnuff = await _csService.GetCurrentSnuffAsync(id);
        if (currentSnuff is null)
        {
            return NotFound();
        }

        Console.WriteLine(currentSnuff);
        Console.WriteLine("--------------------------");

        return currentSnuff;
    }

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("CreateV2")]
    public async Task<ActionResult<CurrentSnuff>> CreateNewCurrentSnuffForUser(CreateCSDto newCurrentSnuff)
    {
        try
        {
           var realCurrentSnuff = await _csService.CreateCurrentSnuffWithDtoAsync(newCurrentSnuff);
            return realCurrentSnuff;
        }

        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("NewSnuffLogV2")]
    public async Task<ActionResult> AddLogV2(string currentsnuffId, int amount)
    {
        Console.WriteLine("in Endpoint AddLogV2");
        try
        {
            await _csService.LogAdderV2(currentsnuffId, amount);
            Console.WriteLine("Log added");
            Console.WriteLine("--------------------------");
            _logger.LogInformation($"Logger data: Amount: {amount} in CurrentSnuff: {currentsnuffId} @ {DateTime.UtcNow}");
            return Ok();
        }

        catch(Exception e)
        {
            _logger.LogError($"Error: {e.Message} @ {DateTime.UtcNow}");
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
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

    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("AddCurrentSnuffToArchive/{csId}")]
    public async Task<IActionResult> AddCurrentSnuffToArchive(string csId)
    {
        try
        {
            Console.WriteLine("Adding to archive");
            Console.WriteLine("--------------------------");
            return Ok(await _csService.AddCurrentSnuffToArchiveAsync(csId));
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetRemainingSnuffInBox/{csId}")]
    public async Task<int> GetRemainingSnuffInBox(string csId)
    {
        try
        {
            Console.WriteLine("Getting remaining snuff in box");
            Console.WriteLine("--------------------------");
            return await _csService.GetAmountInBoxAsync(csId);
        }
        catch
        {
            return 0;
        }
    }

    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("GetSnuffInventory/{uid}")]
    public async Task<List<CurrentSnuffDto>> GetActiveSnuffInventory(string uid)
    {
        var result = await _csService.GetCurrentSnuffInventoryAsync(uid);
        _logger.LogInformation($"Fetch data: {result} {DateTime.UtcNow}");
        return result;
    }
}