using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, bool isHtml = false);

    }
}
