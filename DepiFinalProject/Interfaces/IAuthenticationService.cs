using static DepiFinalProject.DTOs.AuthenticationDTO;

namespace DepiFinalProject.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);
        Task<AuthenticationResponse> LoginAsync(LoginRequest request);
        Task<AuthenticationResponse> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
