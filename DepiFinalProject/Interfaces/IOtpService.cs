using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IOtpService
    {
        Task RequestOtpAsync(User user, string purpose, string destination); // destination: email or phone
        Task<bool> VerifyOtpAsync(User user, string purpose, string code);


    }
}
