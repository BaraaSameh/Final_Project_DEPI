using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.Extensions.Configuration;
using static DepiFinalProject.Core.DTOs.AuthenticationDTO;
using Google.Apis.Auth;

namespace DepiFinalProject.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService; 


        public AuthenticationService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IConfiguration configuration,
            IOtpService otpService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _configuration = configuration;
            _otpService = otpService;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid reset request");
            }

            if (string.IsNullOrEmpty(user.UserPassword))
            {
                throw new InvalidOperationException(
                    "This account was created with Google. Please use Google Sign-In.");
            }

            await _otpService.RequestOtpAsync(user, "passwordreset", email);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otpCode, string newPassword)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid reset request");
            }
            
            var isValidOtp = await _otpService.VerifyOtpAsync(user, "passwordreset", otpCode);

            if (!isValidOtp)
            {
                throw new UnauthorizedAccessException("Invalid or expired OTP code");
            }

            user.UserPassword = _passwordService.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            
            return true;
        }

        public async Task<AuthenticationResponse> GoogleLoginAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["GoogleAuth:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid Google token");
            }

            // Google user info
            string email = payload.Email;
            string firstName = payload.GivenName;
            string lastName = payload.FamilyName;
            string picture = payload.Picture;
            var user = await _userRepository.GetByEmailAsync(email);

            // If user doesn't exist → create one
            if (user == null)
            {
                user = new User
                {
                    UserEmail = email,
                    UserFirstName = firstName,
                    UserLastName = lastName,
                    UserPhone = "0000000000",
                    UserRole = "client",
                    ImageUrl = picture,
                    CreatedAt = DateTime.UtcNow,
                    UserPassword = null 
                };

                user = await _userRepository.CreateAsync(user);
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserID,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.UserEmail))
            {
                throw new InvalidOperationException("Email already registered");
            }

            // Create new user
            var user = new User
            {
                UserEmail = request.UserEmail,
                UserPassword = _passwordService.HashPassword(request.UserPassword),
                UserFirstName = request.UserFirstName,
                UserLastName = request.UserLastName,
                UserPhone = request.UserPhone,
                UserRole = "client",
                CreatedAt = DateTime.UtcNow,
            };

            user = await _userRepository.CreateAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserID,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            return new AuthenticationResponse
            {
                //UserId = user.UserID,
                //UserEmail = user.UserEmail,
                //UserFirstName = user.UserFirstName,
                //UserLastName = user.UserLastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.UserEmail);

            if (user == null || !_passwordService.VerifyPassword(request.UserPassword, user.UserPassword))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserID,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            return new AuthenticationResponse
            {
                //UserId = user.UserID,
                //UserEmail = user.UserEmail,
                //UserFirstName = user.UserFirstName,
                //UserLastName = user.UserLastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task<AuthenticationResponse> GenerateJwtTokenAsync(User user){
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.UserID,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            return new AuthenticationResponse
            {
                //UserId = user.UserID,
                //UserEmail = user.UserEmail,
                //UserFirstName = user.UserFirstName,
                //UserLastName = user.UserLastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
    };
}
        public async Task<AuthenticationResponse> RefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = refreshToken.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

            return new AuthenticationResponse
            {
                //UserId = refreshToken.User.UserID,
                //UserEmail = refreshToken.User.UserEmail,
                //UserFirstName = refreshToken.User.UserFirstName,
                //UserLastName = refreshToken.User.UserLastName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return false;
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.UpdateAsync(refreshToken);
            return true;
        }
    }
}
