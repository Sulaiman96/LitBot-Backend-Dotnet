using LitBot.Core.DTOs.Auth;
using LitBot.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LitBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="userCreateDto">User registration details</param>
    /// <returns>The created user details</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCreateDto userCreateDto)
    {
        try
        {
            logger.LogInformation("User registration attempt for email: {Email}", userCreateDto.Email);
            
            var userResponse = await authService.RegisterUserAsync(userCreateDto);
            
            logger.LogInformation("User registered successfully with ID: {UserId}", userResponse.Id);
            
            return CreatedAtAction(
                nameof(GetCurrentUser), 
                new { }, 
                userResponse);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("already in use"))
        {
            logger.LogWarning("Registration failed - email already exists: {Email}", userCreateDto.Email);
            return Conflict(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = "An account with this email address already exists.",
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Registration failed - invalid input: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during registration. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="userLoginDto">Login credentials</param>
    /// <returns>success message if login successful</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        try
        {
            logger.LogInformation("User login attempt for email: {Email}", userLoginDto.Email);

            await authService.LoginAsync(userLoginDto);

            logger.LogInformation("User logged in successfully");

            return Ok(new {message = "Login Successful"});
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Login failed - invalid input: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Login failed for email: {Email} - {Message}", userLoginDto.Email, ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login");
            return StatusCode(StatusCodes.Status500InternalServerError,  new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during login. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
    
    

    /// <summary>
    /// Get current authenticated user details
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
    {
        try
        {
            logger.LogInformation("Getting current user details");
            
            var user = await authService.GetUserAsync();
            
            logger.LogInformation("Retrieved user details for ID: {UserId}", user.Id);
            
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Get user failed - unauthorized: {Message}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError("Get user failed - HttpContext not available: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "Unable to process request. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting user details");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
}