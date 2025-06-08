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
    /// Refresh the authentication token
    /// </summary>
    /// <returns>Success message if refresh successful</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            logger.LogInformation("Token refresh attempt");
            
            await authService.RefreshTokenAsync();
            
            logger.LogInformation("Token refreshed successfully");
            
            return Ok(new { message = "Token refreshed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Token Refresh Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError("Token refresh failed - HttpContext not available: {Message}", ex.Message);
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
            logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
    
    /// <summary>
    /// Request a password reset email
    /// </summary>
    /// <param name="forgotPasswordDto">Email address for password reset</param>
    /// <returns>Success message</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordDto)
    {
        try
        {
            logger.LogInformation("Password reset requested for email: {Email}", forgotPasswordDto.Email);
            
            var message = await authService.ForgotPasswordAsync(forgotPasswordDto);
            
            logger.LogInformation("Password reset email sent to: {Email}", forgotPasswordDto.Email);
            
            return Ok(new { message = message });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Forgot password failed - invalid input: {Message}", ex.Message);
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
            logger.LogError(ex, "Unexpected error during password reset request");
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }
    }
    
    /// <summary>
    /// Reset password using token from email
    /// </summary>
    /// <param name="resetPasswordDto">Token and new password</param>
    /// <returns>Success message</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordDto)
    {
        try
        {
            logger.LogInformation("Password reset attempt with token");
            
            var message = await authService.ResetPasswordAsync(resetPasswordDto);
            
            logger.LogInformation("Password reset successful");
            
            return Ok(new { message = message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Password reset failed - invalid token: {Message}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Password Reset Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during password reset");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
    
    /// <summary>
    /// Change password using current password
    /// </summary>
    /// <param name="changePasswordDto">contains current and new password</param>
    /// <returns>successfully changes password</returns>
    [HttpPost("change-password")]
    [Authorize] // User must be logged in
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
    {
        try
        {
            logger.LogInformation("Password change attempt for authenticated user");
            
            var message = await authService.ChangePasswordAsync(changePasswordDto);
            
            logger.LogInformation("Password changed successfully");
            
            return Ok(new { message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Password change failed: {Message}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Password Change Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during password change");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
    
    /// <summary>
    /// Logout the current user
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        try
        {
            logger.LogInformation("User logout");
            
            // Clear the authentication cookies
            Response.Cookies.Delete("sb-access-token");
            Response.Cookies.Delete("sb-refresh-token");
            
            logger.LogInformation("User logged out successfully");
            
            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Ok(new { message = "Logout completed" });
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