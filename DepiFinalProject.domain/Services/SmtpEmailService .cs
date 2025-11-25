using DepiFinalProject.Core.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;


namespace DepiFinalProject.Services
{

    public class SmtpSettings {
        public string Host  { get; set; }
        public int Port { get; set; } 
        public string From { get; set; } 
        public string User { get; set; } 
        public string Pass { get; set; }
        public bool UseSsl { get; set; } 
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        public SmtpEmailService(IOptions<SmtpSettings> opts) => _settings = opts.Value;

        public async Task SendAsync(string to, string subject, string body, bool isHtml = false)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_settings.From));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;

            msg.Body = new TextPart(isHtml ? "html" : "plain")
            {
                Text = body
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                MailKit.Security.SecureSocketOptions.StartTls
            );

            if (!string.IsNullOrEmpty(_settings.User))
                await client.AuthenticateAsync(_settings.User, _settings.Pass);

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }

    }
}
