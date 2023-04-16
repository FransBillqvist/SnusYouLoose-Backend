using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnuffController : ControllerBase
{
    private readonly ILogger<SnuffController> _logger;
    private readonly IGenericMongoRepository<Snuff> _snuffRepository;

    public SnuffController(ILogger<SnuffController> logger, IGenericMongoRepository<Snuff> snuffRepository)
    {
        _logger = logger;
        _snuffRepository = snuffRepository;
    }

    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<Snuff>> GetSnuff(string id)
    {
        try
        {
            var response = await _snuffRepository.FindOneAsync(x => x.Id == id);

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
    public async Task<IActionResult> Post(Snuff newSnuff)
    {
        try
        {
            await _snuffRepository.InsertOneAsync(newSnuff);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, Snuff updatedSnuff)
    {
        try
        {
            var snuff = await _snuffRepository.FindByIdAsync(id);

            if (snuff is null)
            {
                return NotFound();
            }

            updatedSnuff.Id = snuff.Id;

            await _snuffRepository.ReplaceOneAsync(updatedSnuff);

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
        var snuff = await _snuffRepository.FindByIdAsync(id);

        if (snuff is null)
        {
            return NotFound();
        }

        await _snuffRepository.DeleteByIdAsync(id);

        return NoContent();
    }
}
