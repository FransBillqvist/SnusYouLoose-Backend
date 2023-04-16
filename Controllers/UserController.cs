using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IGenericMongoRepository<User> _userRepository;

    public UserController(ILogger<UserController> logger, IGenericMongoRepository<User> userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    // [HttpGet]
    // public async Task<List<User>> Get() => await _userService.GetAllUsersAsync();

    [HttpGet]
    [Route("TEST")]
    public async Task<ActionResult<User>> GetUserData(string id)
    {
        var response = await _userRepository.FindOneAsync(x => x.Id == id);

        if (response is null)
        {
            return null;
        }

        return response;
    }


    [HttpGet]
    [Route("Get/{Id}")]
    public IEnumerable<string> GetUserDataById(string id)
    {
        var hej = ObjectId.Parse(id);
        var users = _userRepository.FilterBy(
            filter => filter.Id == "id",
            projection => projection.Name
        );

        if (users is null)
        {
            return null;
        }

        return users;
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(User newUser)
    {
        try
        {
            await _userRepository.InsertOneAsync(newUser);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Route("Update/{id}")]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            updatedUser.Id = user.Id;

            await _userRepository.ReplaceOneAsync(updatedUser);

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
        var user = await _userRepository.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        await _userRepository.DeleteByIdAsync(id);

        return NoContent();
    }


}
