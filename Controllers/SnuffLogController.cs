using DAL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class SnuffLogController : ControllerBase
{
    private readonly ILogger<SnuffLogController> _logger;
    private readonly SnuffLogService _snuffLogService;

    public SnuffLogController(ILogger<SnuffLogController> logger, SnuffLogService snuffLogService)
    {
        _logger = logger;
        _snuffLogService = snuffLogService;
    }

    [HttpGet]
    public async Task<List<SnuffLog>> Get() => await _snuffLogService.GetAllSnuffLogsAsync();

    [HttpGet]
    public async Task<ActionResult<SnuffLog>> Get(string id)
    {
        var snuffLog = await _snuffLogService.GetSnuffLogAsync(id);

        if (snuffLog is null)
        {
            return NotFound();
        }

        return snuffLog;
    }

    [HttpPost]
    public async Task<IActionResult> Post(SnuffLog newSnuffLog)
    {
        await _snuffLogService.CreateSnuffLogAsync(newSnuffLog);

        return CreatedAtAction(nameof(Get), new { id = newSnuffLog.Id }, newSnuffLog);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, SnuffLog updatedSnuffLog)
    {
        var snuffLog = await _snuffLogService.GetSnuffLogAsync(id);

        if (snuffLog is null)
        {
            return NotFound();
        }

        updatedSnuffLog.Id = snuffLog.Id;

        await _snuffLogService.UpdateSnuffLogAsync(id, updatedSnuffLog);

        return NoContent();
    }

    [HttpDelete]
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
