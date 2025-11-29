using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.IdentityModel.Tokens;

public interface IInvoiceTokenService
{
    string GenerateToken(Payment payment);
    Payment? ValidateAndConsumeToken(string token);
}

public class InvoiceTokenService : IInvoiceTokenService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly string _secret;

    public InvoiceTokenService(Microsoft.Extensions.Configuration.IConfiguration config, IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
        _secret = config["Jwt:Key"] ?? config["OtpSecret"] ?? "ZenonMarketSuperSecret2025!ThisIs32+CharsLong!";

        // HMAC-SHA256 requires at least 16 bytes (128 bits), better 32+)
        if (Encoding.UTF8.GetByteCount(_secret) < 32)
            throw new ArgumentException("JWT secret must be at least 32 characters for HS256");
    }

    public string GenerateToken(Payment payment)
    {
        var claims = new[]
        {
            new Claim("pid", payment.PayPalOrderId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "ZenonMarket",
            audience: "ZenonMarket",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(48),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Payment? ValidateAndConsumeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "ZenonMarket",
                ValidAudience = "ZenonMarket",
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(5)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var pid = jwtToken.Claims.FirstOrDefault(x => x.Type == "pid")?.Value;

            if (string.IsNullOrEmpty(pid))
                return null;

            var payment = _paymentRepo.GetByIdAsync(pid).Result;
            if (payment == null || payment.Status?.ToLower() != "completed")
                return null;

             

            return payment;
        }
        catch
        {
            return null;
        }
    }
}