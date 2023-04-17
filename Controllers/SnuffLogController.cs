using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Services;
using Services.Interfaces;

namespace Controllers.SnuffLogController;

[ApiController]
[Route("api/[controller]")]
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
            var response = await _snuffLogRepository.FindOneAsync(x => x.Id == id);

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
    public async Task<IActionResult> Post(SnuffLog newSnuffLog)
    {
        try
        {
            await _snuffLogService.CreateSnuffLogAsync(newSnuffLog);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
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
        var snuffLog = await _snuffLogRepository.FindByIdAsync(id);

        if (snuffLog is null)
        {
            return NotFound();
        }

        await _snuffLogRepository.DeleteByIdAsync(id);

        return NoContent();
    }
}
