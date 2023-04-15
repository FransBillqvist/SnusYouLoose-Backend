using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Services;

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

    // [HttpGet]
    // [Route("user/{id}")]
    // public async Task<ActionResult<User>> Get(string id)
    // {
    //     var user = await _userRepository.GetUserAsync(id);

    //     if (user is null)
    //     {
    //         return NotFound();
    //     }

    //     return user;
    // }

    [HttpPost]
    [Route("user/new")]
    public async Task<IActionResult> Post(User newUser)
    {
        await _userRepository.InsertOneAsync(newUser);

        return CreatedAtAction(nameof(User), new { id = newUser.Id }, newUser);
    }

    // [HttpPut]
    // [Route("user/update/{id}")]
    // public async Task<IActionResult> Update(string id, User updatedUser)
    // {
    //     var user = await _userRepository.GetUserAsync(id);

    //     if (user is null)
    //     {
    //         return NotFound();
    //     }

    //     updatedUser.Id = user.Id;

    //     await _userRepository.UpdateUserAsync(updatedUser);

    //     return NoContent();
    // }

    // [HttpDelete]
    // [Route("user/delete/{id}")]
    // public async Task<IActionResult> Delete(string id)
    // {
    //     var user = await _userRepository.GetUserAsync(id);

    //     if (user is null)
    //     {
    //         return NotFound();
    //     }

    //     await _userRepository.RemoveUserAsync(id);

    //     return NoContent();
    // }


}
