using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
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
    [MapToApiVersion("2.0")]
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        try
        {
            var response = await _userRepository.FindOneAsync(x => x.UserId == id);

            if (response is null)
            {
                Console.WriteLine("Inside Get Null");
                return NotFound();
            }
            Console.WriteLine($"Get User data for User: {response.Username}");
            Console.WriteLine($"------------------------------------------");
            return response;
        }
        catch
        {
            return NotFound();
        }
    }
    [MapToApiVersion("2.0")]
    [HttpPost]
    [Route("CreateV2")]
    public async Task<IActionResult> Post(User newUser)
    {
        try
        {
            newUser.CreatedAtUtc = DateTime.UtcNow;
            await _userService.CreateUserV2Async(newUser);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }
    [MapToApiVersion("2.0")]
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
    [MapToApiVersion("2.0")]
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
