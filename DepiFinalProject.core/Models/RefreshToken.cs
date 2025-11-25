namespace DepiFinalProject.Core.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public virtual User User { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
    }
}
