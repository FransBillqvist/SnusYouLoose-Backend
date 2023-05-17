using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<Progression>> GetProgression(string id)
    {
        try
        {
            var response = await _progressionService.FindUserActiveProgression(id);

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
    public async Task<ActionResult<int>> GetRemainingSnuffToday(string id)
    {
        try
        {
            var response = await _progressionService.CalculateRemaingSnuff(id);

            if (response is null)
            {
                return NotFound();
            }

            return response.RemainingSnuffToday;
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
            await _progressionService.UpdateInUseState(newState);
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
}