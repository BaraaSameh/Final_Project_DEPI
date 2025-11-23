using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using DepiFinalProject.Repositories;
using static DepiFinalProject.DTOs.AuthenticationDTO;

namespace DepiFinalProject.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _configuration = configuration;
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
