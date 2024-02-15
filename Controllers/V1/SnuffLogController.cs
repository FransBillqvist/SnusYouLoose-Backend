using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.SnuffLogController;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SnuffLogController : ControllerBase
{
    private readonly ILogger<SnuffLogController> _logger;
    private readonly IGenericMongoRepository<SnuffLog> _snuffLogRepository;
    private readonly ISnuffLogService _snuffLogService;

    public SnuffLogController(ILogger<SnuffLogController> logger, IGenericMongoRepository<SnuffLog> snuffLogRepository, ISnuffLogService snuffLogService)
    {
        _logger = logger;
        _snuffLogRepository = snuffLogRepository;
        _snuffLogService = snuffLogService;
    }


    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<SnuffLog>> GetSnuffLog(string id)
    {
        try
        {
            var response = await _snuffLogService.GetSnuffLogAsync(id);

            if (response != null)
            {
                return response;
            }

            return NotFound();
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(SnuffLog newSnuffLog)
    {
        try
        {
            Console.WriteLine("CreateSnuffLog line 51: amount of snuff to use: " + newSnuffLog.AmountUsed);
            newSnuffLog.CreatedAtUtc = DateTime.UtcNow.IsDaylightSavingTime() ? DateTime.UtcNow.AddHours(2) : DateTime.UtcNow.AddHours(1);;
            await _snuffLogService.CreateSnuffLogAsync(newSnuffLog);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Obsolete]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, SnuffLog updatedSnuffLog)
    {
        try
        {
            var snuffLog = await _snuffLogRepository.FindByIdAsync(id);

            if (snuffLog is null)
            {
                return NotFound();
            }

            updatedSnuffLog.Id = snuffLog.Id;

            await _snuffLogRepository.ReplaceOneAsync(updatedSnuffLog);

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
        var snuffLog = await _snuffLogService.GetSnuffLogAsync(id);

        if (snuffLog is null)
        {
            return NotFound();
        }

        await _snuffLogService.RemoveSnuffLogAsync(id);

        return NoContent();
    }
}
