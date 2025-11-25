using DepiFinalProject.Core.Models;
using static DepiFinalProject.Core.DTOs.AuthenticationDTO;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);
        Task<AuthenticationResponse> LoginAsync(LoginRequest request);
        Task<AuthenticationResponse> RefreshTokenAsync(string token);
        Task<AuthenticationResponse> GenerateJwtTokenAsync(User user);
        Task<bool> RevokeTokenAsync(string token);
    }
}
