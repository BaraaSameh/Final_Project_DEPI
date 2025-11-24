using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.AuthenticationDTO;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IOtpService _otpService;
        private readonly IUserRepository _userRepo;
        public AuthenticationController(IAuthenticationService authService, IOtpService otpService, IUserRepository userRepo)
        {
            _authService = authService;
            _otpService = otpService;
            _userRepo = userRepo;
        }

        [HttpPost("request-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Purpose)) return BadRequest("Invalid request");

            User user = null;
            if (!string.IsNullOrEmpty(dto.Email))
                user = await _userRepo.GetByEmailAsync(dto.Email);
            else if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);

            if (user == null) return NotFound("User not found");

            var destination = !string.IsNullOrEmpty(dto.Email) ? dto.Email :" ";

            await _otpService.RequestOtpAsync(user, dto.Purpose, destination);

            return Ok(new { message = "OTP sent" });
        }
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Code)) return BadRequest("Invalid request");

            User user = null;
            if (!string.IsNullOrEmpty(dto.Email))
                user = await _userRepo.GetByEmailAsync(dto.Email);
            else if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);

            if (user == null) return NotFound("User not found");

            var ok = await _otpService.VerifyOtpAsync(user, dto.Purpose, dto.Code);
            if (!ok) return BadRequest("Invalid or expired code");

            if (dto.Purpose == "login")
            {
                var response = await _authService.GenerateJwtTokenAsync(user);
                SetRefreshTokenCookie(response.RefreshToken);
                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    response, "OTP verified and login successful"));

            }

            return Ok(new { message = "OTP verified" });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                    ex.Message, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during registration",
                    new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                    ex.Message, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during login",
                    new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                    ex.Message, new List<string> { ex.Message, ex.InnerException?.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthenticationResponse>.ErrorResponse(
                    "An error occurred during token refresh",
                    new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

        [HttpPost("revoke-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                    "An error occurred during token revocation",
                    new List<string> { ex.Message, ex.InnerException?.Message }));
            }
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(14)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
