using LitBot.Core.DTOs.Auth;
using LitBot.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LitBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> Register(UserCreateDto userCreateDto)
    {
        try
        {
            var userResponseDto = await authService.RegisterUserAsync(userCreateDto);
            return CreatedAtAction(nameof(GetUser), new { }, userResponseDto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new {message = ex.Message});
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new {message = ex.Message});
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

}