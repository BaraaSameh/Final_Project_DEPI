using System.Security.Cryptography;
using System.Text;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using DepiFinalProject.Infrastructurenamespace.Repositories;
using Microsoft.Extensions.Configuration;
using PaypalServerSdk.Standard.Models;

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
        private readonly IOrderService _orderService;
        private readonly IInvoiceTokenService _tokenService;
        public OtpService(IOtpRepository otpRepo, IEmailService emailService, IUserRepository userRepo, IConfiguration config, IOrderService orderService, IInvoiceTokenService tokenService)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
            _userRepo = userRepo;
            _otpSecret = config["OtpSecret"] ?? throw new ArgumentNullException("OtpSecret missing in config");
            _orderService = orderService;
            _tokenService = tokenService;
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

        public async Task SendPaymentInvoiceAsync(Payment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (!payment.OrderID.HasValue)
                throw new InvalidOperationException("Payment must be associated with an order.");

             var user = await _userRepo.GetByIdAsync(payment.UserId);
            if (user == null) throw new InvalidOperationException("User not found for this payment.");

            var order = await _orderService.GetByIdAsync(payment.OrderID.Value);
            if (order == null) throw new InvalidOperationException("Order not found.");

            var customerName = $"{user.UserFirstName} {user.UserLastName}".Trim();
            if (string.IsNullOrEmpty(customerName)) customerName = "عزيزي العميل";
            var token = _tokenService.GenerateToken(payment);
            var invoiceUrl = $"https://zenon-ecomm-mega-project.vercel.app/invoice.html?token={token}"; 

            var message = $@"
<div style='font-family: Arial, sans-serif; background:#f8f9fc; padding:50px 0; text-align:center; direction:rtl;'>
    <div style='background:#ffffff; max-width:560px; margin:0 auto; padding:40px 30px;
                border-radius:16px; box-shadow:0 10px 30px rgba(0,0,0,0.08); border-top:6px solid #667eea;'>
        
        <h1 style='color:#003366; margin-top:0; font-size:28px; font-weight:bold;'>
            تم الدفع بنجاح!
        </h1>
        
        <div style='background:#e8f5e8; color:#28a745; padding:15px; border-radius:12px; font-size:18px; font-weight:bold; margin:25px 0;'>
            تم استلام دفعك بأمان عبر PayPal
        </div>

        <p style='color:#333; font-size:17px; line-height:1.8; margin:20px 0;'>
            مرحباً <strong>{customerName}</strong>،<br><br>
            نشكرك على ثقتك في <strong>Zenon Market</strong>.<br>
            تم استلام دفع طلبك رقم <strong>#{order.OrderNo}</strong> بنجاح.
        </p>

        <div style='background:#f8f9ff; padding:25px; border-radius:12px; margin:30px 0; text-align:right; font-size:16px;'>
            <div style='margin:12px 0;'><strong>المبلغ المدفوع:</strong> <span style='color:#667eea; font-size:22px;'>${payment.Amount:F2}</span></div>
            <div style='margin:12px 0;'><strong>طريقة الدفع:</strong> PayPal</div>
            <div style='margin:12px 0;'><strong>تاريخ الدفع:</strong> {payment.PaidAt:dddd, d MMMM yyyy - hh:mm tt}</div>
            <div style='margin:12px 0;'><strong>رقم المعاملة:</strong> {payment.PaymentID}</div>
        </div>

        <a href='{invoiceUrl}' style='
            background:#667eea;
            color:white;
            padding:16px 40px;
            font-size:18px;
            font-weight:bold;
            text-decoration:none;
            border-radius:12px;
            display:inline-block;
            margin:20px 0;
            box-shadow:0 6px 15px rgba(102,126,234,0.3);
        '>
            عرض الفاتورة الإلكترونية
        </a>

        <p style='color:#666; font-size:14px; line-height:1.8; margin-top:30px;'>
            سيتم تجهيز طلبك وشحنه في أقرب وقت<br>
            يمكنك متابعة حالة الطلب من حسابك في الموقع.
        </p>
    </div>

    <div style='color:#999; font-size:13px; margin-top:30px;'>
        © 2025 Zenon Market — جميع الحقوق محفوظة<br>
        هذه رسالة تلقائية، يرجى عدم الرد عليها.
    </div>
</div>";

            await _emailService.SendAsync(
                to: user.UserEmail,
                subject: $"تم دفع طلبك بنجاح - Zenon Market #{order.OrderNo}",
                body: message,
                isHtml: true
            );
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