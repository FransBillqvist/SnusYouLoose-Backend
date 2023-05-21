using System;
using Microsoft.AspNetCore.Mvc;
using DAL.Dto;
using Microsoft.AspNetCore.Identity;
using DAL;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public AuthController(UserManager<AuthUser> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    [Route("roles/add")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var appRole = new Role
        {
            Name = request.Role
        };
        var createRole = await _roleManager.CreateAsync(appRole);

        return Ok(new { message = "Role created successfully" });
    }

    [HttpPost]
    [Route("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await RegisterAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await LoginAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    private async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return new LoginResponse { Message = "Invalid Email/Password", Success = false };

            //all is well if we reach this point

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));
            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("B055355up3R53Cr3tK3y@345"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(14);

            var token = new JwtSecurityToken(
                issuer: "http://localhost:5126",
                audience: "http://localhost:5126",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Message = "Success",
                Email = user.Email,
                FullName = user.FullName,
                Success = true,
                UserId = user.Id.ToString(),
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new LoginResponse { Message = ex.Message, Success = false };
        }
    }

    private async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null) return new RegisterResponse { Message = "User already exists!", Success = false };

            var newUser = new AuthUser
            {
                FullName = request.FullName,
                Email = request.Email,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                UserName = request.Email,
            };

            var createUserResult = await _userManager.CreateAsync(newUser, request.Password);
            if (!createUserResult.Succeeded) return new RegisterResponse
            {
                Message = $"Failed to create user! {createUserResult.Errors.First().Description}",
                Success = false
            };

            var addUserToRole = await _userManager.AddToRoleAsync(newUser, "USER");
            if (!addUserToRole.Succeeded) return new RegisterResponse
            {
                Message = $"Failed to add user to role! {addUserToRole.Errors.First().Description}",
                Success = false
            };

            return new RegisterResponse { Id = newUser.Id.ToString(), Message = "User is now registered", Success = true };

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new RegisterResponse { Message = ex.Message, Success = false };
        }
    }
}