using Services;
using Microsoft.AspNetCore.Mvc;
using DAL;
using MongoDB.Bson;
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

    //     [HttpGet]
    //     public async Task<List<CurrentSnuff>> Get() => await _currentSnuffService.GetAllCurrentSnuffAsync();

    [HttpGet]
    [Route("get/{id}")]
    public async Task<ActionResult<CurrentSnuff>> Get(string id)
    {
        var currentSnuff = await _csRepository.FindOneAsync(x => x.Id == id);

        if (currentSnuff is null)
        {
            return NotFound();
        }

        return currentSnuff;
    }

    [HttpPost]
    [Route("post")]
    public async Task<IActionResult> Post(CurrentSnuff newCurrentSnuff)
    {
        await _csRepository.InsertOneAsync(newCurrentSnuff);

        return CreatedAtAction(nameof(Get), new { id = newCurrentSnuff.Id }, newCurrentSnuff);
    }

    //     [HttpPut]
    //     public async Task<IActionResult> Update(string id, CurrentSnuff updatedCurrentSnuff)
    //     {
    //         ObjectId mongoId = ObjectId.Parse(id);
    //         var currentSnuff = await _currentSnuffService.GetCurrentSnuffAsync(mongoId);

    //         if (currentSnuff is null)
    //         {
    //             return NotFound();
    //         }

    //        updatedCurrentSnuff.Id = currentSnuff.Id;

    //         await _currentSnuffService.UpdateCurrentSnuffAsync(mongoId, updatedCurrentSnuff);

    //         return NoContent();
    //     }

    //     [HttpDelete]
    //     public async Task<IActionResult> Delete(string id)
    //     {
    //         ObjectId mongoId = ObjectId.Parse(id);
    //         var currentSnuff = await _currentSnuffService.GetCurrentSnuffAsync(mongoId);

    //         if (currentSnuff is null)
    //         {
    //             return NotFound();
    //         }

    //         await _currentSnuffService.RemoveCurrentSnuffAsync(mongoId);

    //         return NoContent();
    //     }
}