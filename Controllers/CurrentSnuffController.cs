using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.Interfaces;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrentSnuffController : ControllerBase
{

    private readonly ILogger<CurrentSnuffController> _logger;
    private readonly IGenericMongoRepository<CurrentSnuff> _csRepository;

    public CurrentSnuffController(ILogger<CurrentSnuffController> logger, IGenericMongoRepository<CurrentSnuff> CSRepository)
    {
        _logger = logger;
        _csRepository = CSRepository;
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

    [HttpPut]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, CurrentSnuff updatedCurrentSnuff)
    {

        try
        {
            var currentSnuff = await _csRepository.FindByIdAsync(id);

            if (currentSnuff is null)
            {
                return NotFound();
            }

            updatedCurrentSnuff.Id = currentSnuff.Id;

            await _csRepository.ReplaceOneAsync(updatedCurrentSnuff);

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