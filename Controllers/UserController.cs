using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IGenericMongoRepository<User> _userRepository;
    private readonly IUserService _userService;

    public UserController(
            ILogger<UserController> logger,
            IGenericMongoRepository<User> userRepository,
            IUserService userService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _userService = userService;
    }

    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        try
        {
            var response = await _userRepository.FindOneAsync(x => x.Id == id);

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

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Post(User newUser)
    {
        try
        {
            await _userService.CreateUserAsync(newUser);
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
            var user = await _userService.GetUserAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            updatedUser.Id = user.Id;

            await _userService.UpdateUserAsync(updatedUser);

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
        var user = await _userService.GetUserAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveUserAsync(id);

        return NoContent();
    }
}
