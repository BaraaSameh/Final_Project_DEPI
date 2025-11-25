using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using DepiFinalProject.Infrastructurenamespace.Repositories;

namespace DepiFinalProject.Services
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepo;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepo;
        private readonly string _otpSecret;
        private readonly int _otpLength = 6;
        private readonly int _expiryMinutes = 5;
        private readonly int _maxAttempts = 5;

        public OtpService(IOtpRepository otpRepo, IEmailService emailService, IUserRepository userRepo, IConfiguration config)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
            _userRepo = userRepo;
            _otpSecret = config["OtpSecret"] ?? throw new ArgumentNullException("OtpSecret missing in config");
        }

        private string GenerateNumericCode(int length)
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append((b % 10).ToString()); 
            return sb.ToString();
        }

        private string HashOtp(string otp)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_otpSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }

        public async Task RequestOtpAsync(User user, string purpose, string destination)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var allowed = new[] { "login", "passwordreset", "emailverification", "payment" };
            purpose = purpose.ToLower();
            if (!allowed.Contains(purpose))
                throw new ArgumentException("Invalid OTP purpose");


            var code = GenerateNumericCode(_otpLength);
            var hash = HashOtp(code);

            var entry = new OTP
            {
                UserId = user.UserID,
                Purpose = purpose,
                OtpHash = hash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                IsUsed = false
            };

            await _otpRepo.AddAsync(entry);
            await _otpRepo.SaveChangesAsync();
            var name = user.UserFirstName + ' ' + user.UserLastName;
             var message = $@"
<div style='font-family: Arial, sans-serif; background:#f5f6f8; padding:40px 0; text-align:center;'>

    <div style='background:#ffffff; max-width:520px; margin:0 auto; padding:35px 30px; 
                border-radius:12px; box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

        <h2 style='color:#003366; margin-top:0; font-size:24px;'>
            Verification Needed
        </h2>

        <p style='color:#333; font-size:16px; margin-bottom:10px;'>
        Hi <strong>{name ?? "there"}</strong>,
        </p>

        <p style='color:#444; font-size:15px; margin-bottom:25px;'>
            Please use the following code to continue with your <strong>{purpose}</strong>:
        </p>

        <div style='
            background:#003366;
            color:#ffffff;
            display:inline-block;
            padding:20px 45px;
            font-size:38px;
            font-weight:bold;
            letter-spacing:10px;
            border-radius:10px;
            margin-bottom:25px;
        '>
            {code}
        </div>

        <p style='color:#666; font-size:14px; line-height:1.7; margin-top:10px;'>
            This verification code will expire in <strong>{_expiryMinutes} minutes</strong>.<br>
            For your security, please do not share this code with anyone.
        </p>

    </div>

    <p style='color:#999; font-size:12px; margin-top:25px;'>
        © 2025 DEPI PROJECT — Secure Login System<br>
        This is an automated message. Please do not reply.
    </p>

</div>
";


            if (!string.IsNullOrEmpty(user.UserEmail) && destination.Contains("@"))
            {
                await _emailService.SendAsync(
                    user.UserEmail,
                    $"Your {purpose} Verification Code",
                    message,
                    isHtml: true
                );
            }
            else
            {
                throw new InvalidOperationException("No valid destination to send OTP.");
            }
        }


        public async Task<bool> VerifyOtpAsync(User user, string purpose, string code)
        {
            if (user == null) return false;
            var allowed = new[] { "login", "passwordreset", "emailverification", "payment" };
            purpose = purpose.ToLower();
            if (!allowed.Contains(purpose))
                throw new ArgumentException("Invalid OTP purpose");


            var otp = await _otpRepo.GetValidOtpAsync(user.UserID, purpose);

            if (otp == null)
                return false;

            if (otp.Attempts >= _maxAttempts)
                return false;

            if (otp.ExpiresAt < DateTime.UtcNow)
                return false;

            var hashed = HashOtp(code);
            var isCorrect = otp.OtpHash == hashed;

            if (!isCorrect)
            {
                otp.Attempts += 1;
                await _otpRepo.SaveChangesAsync();
                return false;
            }

            otp.IsUsed = true;
            await _otpRepo.SaveChangesAsync();

            return true;
        }

    }
}