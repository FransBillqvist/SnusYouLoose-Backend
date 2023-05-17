using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]

public class ProgressionController : ControllerBase
{

    private readonly ILogger<ProgressionController> _logger;
    private readonly IGenericMongoRepository<Progression> _progressionRepository;
    private readonly IProgressionService _progressionService;

    public ProgressionController(
            ILogger<ProgressionController> logger,
            IGenericMongoRepository<Progression> progressionRepository,
            IProgressionService progressionService)
    {
        _logger = logger;
        _progressionRepository = progressionRepository;
        _progressionService = progressionService;
    }

    [HttpGet]
    [Route("GetProgression/{id}")]
    public async Task<ActionResult<Progression>> GetCurrentUserProgression(string uid)
    {
        try
        {
            var response = await _progressionService.FindUserActiveProgression(uid);

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

    [HttpGet]
    [Route("RemaingSnuffToday/{id}")]
    public async Task<ActionResult<int>> GetRemainingSnuffToday(string uid)
    {
        try
        {
            var response = await _progressionService.CalculateRemaingSnuff(uid);

            return response;
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPut]
    [Route("UpdateInUseProgression")]
    public async Task<IActionResult> UpdateInUseProgression(Progression newState)
    {
        try
        {
            await _progressionService.UpdateProgressionStateAsync(newState);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPost]
    [Route("CreateUserProgression")]
    public async Task<IActionResult> CreateUserProgression(Progression dto)
    {
        try
        {
            await _progressionService.AddNewProgression(dto);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpDelete]
    [Route("RemoveProgression/{id}")]
    public async Task<IActionResult> RemoveProgression(string uid)
    {
        try
        {
            await _progressionService.RemoveProgressionAsync(uid);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }
}