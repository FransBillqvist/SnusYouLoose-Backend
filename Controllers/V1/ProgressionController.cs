using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]

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
    [Route("GetProgression/{uid}")]
    public async Task<ActionResult<Progression>> GetCurrentUserProgression(string uid)
    {
        Console.WriteLine("I'M GetCurrentUserProgression with userId: " + uid);
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
    [Route("RemainingSnuffToday/{uid}")]
    public async Task<ActionResult<int>> GetRemainingSnuffToday(string uid)
    {
        try
        {
            Console.WriteLine("ProgressionController: inside remainingsnufftoday, id is: " + uid);
            var response = await _progressionService.CalculateRemainingSnuff(uid);
            Console.WriteLine("amount of snuff left: " + response);
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
    public async Task<ActionResult<Progression>> CreateUserProgression(string uid)
    {
        Console.WriteLine("I'M CreateUserProgression with userId: " + uid);
        try
        {
            var newProgression = await _progressionService.AddNewProgression(uid);

            return newProgression;
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

    [HttpGet]
    [Route("TimeToNextDose/{uid}")]
    public async Task<ActionResult<string>> TimeToNextDose(string uid)
    {
        try
        {
            Console.WriteLine("I'M WhenIsTheNextDoseAvailable with userId: " + uid + "time is: " + DateTime.Now);
            var result = await _progressionService.WhenIsTheNextDoseAvailable(uid);
            Console.WriteLine("I am the result to send out! " + result);
            return result;
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("LastConsumedSnuff/{uid}")]
    public async Task<ActionResult<string>> LastConsumedSnuff(string uid)
    {
        try
        {
            Console.WriteLine("I'M LastConsumedSnuff with userId: " + uid);
            var result = await _progressionService.LastConsumedSnuffAtUtc(uid);
            return result;
        }
        catch (InvalidOperationException ex)
        {
            // Hantera undantag som InvalidOperationException här
            // Exempel: returnera en NotFound-response eller ett anpassat felmeddelande
            return NotFound("Specified user not found.");
        }
        catch (Exception ex)
        {
            // Hantera andra undantag här
            // Exempel: logga felmeddelandet eller returnera en InternalServerError-response
            Console.WriteLine("An error occurred: " + ex.Message);
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}