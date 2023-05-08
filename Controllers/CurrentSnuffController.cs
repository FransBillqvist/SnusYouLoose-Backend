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
    [Route("Get/{id}")]
    public async Task<ActionResult<CurrentSnuff>> GetCurrentSnuff(string id)
    {
        var currentSnuff = await _csRepository.FindOneAsync(x => x.Id == id);

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
            await _csRepository.InsertOneAsync(newCurrentSnuff);
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
            var currentSnuff = await _csRepository.FindByIdAsync(id);

            if (currentSnuff is null)
            {
                return NotFound();
            }

            await _csRepository.DeleteByIdAsync(id);
            return NoContent();
        }

        catch
        {
            return BadRequest();
        }
    }
}