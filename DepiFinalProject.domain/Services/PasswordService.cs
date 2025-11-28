using BCrypt.Net;
using DepiFinalProject.Core.Interfaces;
namespace DepiFinalProject.Services
{
    public class PasswordService:IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password,15);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
