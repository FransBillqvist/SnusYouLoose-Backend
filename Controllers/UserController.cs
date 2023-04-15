using DAL;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserService _userService;

    public UserController(ILogger<UserController> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<List<User>> Get() => await _userService.GetAllUsersAsync();

    [HttpGet]
    public async Task<ActionResult<User>> Get(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        var user = await _userService.GetUserAsync(mongoId);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<IActionResult> Post(User newUser)
    {
        await _userService.CreateUserAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        var user = await _userService.GetUserAsync(mongoId);

        if (user is null)
        {
            return NotFound();
        }

        updatedUser.Id = user.Id;

        await _userService.UpdateUserAsync(mongoId, updatedUser);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        ObjectId mongoId = ObjectId.Parse(id);
        var user = await _userService.GetUserAsync(mongoId);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveUserAsync(mongoId);

        return NoContent();
    }

   
}
