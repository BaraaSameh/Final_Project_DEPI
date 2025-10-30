using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.AuthenticationDTO;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                SetRefreshTokenCookie(response.RefreshToken);

                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    response, "User registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthenticationResponse>.ErrorResponse(
                    ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during registration", new List<string> { ex.Message }));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                SetRefreshTokenCookie(response.RefreshToken);

                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    response, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthenticationResponse>.ErrorResponse(
                    ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during login", new List<string> { ex.Message }));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var token = request.RefreshToken ?? Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(ApiResponse<AuthenticationResponse>.ErrorResponse(
                        "Refresh token is required", new List<string> { "Token not provided" }));
                }

                var response = await _authService.RefreshTokenAsync(token);
                SetRefreshTokenCookie(response.RefreshToken);

                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    response, "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthenticationResponse>.ErrorResponse(
                    ex.Message, new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during token refresh", new List<string> { ex.Message }));
            }
        }

        [HttpPost("revoke-token")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var token = request.RefreshToken ?? Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse(
                        "Refresh token is required", new List<string> { "Token not provided" }));
                }

                var result = await _authService.RevokeTokenAsync(token);

                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        "Token not found or already revoked", new List<string> { "Invalid token" }));
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Token revoked successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "An error occurred during token revocation", new List<string> { ex.Message }));
            }
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

             Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        
    }
}
