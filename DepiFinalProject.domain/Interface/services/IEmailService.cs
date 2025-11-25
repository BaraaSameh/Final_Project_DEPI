using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, bool isHtml = false);

    }
}
