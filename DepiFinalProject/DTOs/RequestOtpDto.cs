using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs { 
    public class RequestOtpDto
    {
        public string Email { get; set; }       // either email or phone must be present
        public string PhoneNumber { get; set; }
        public string Purpose { get; set; }     // "login" or "payment"

    }
    public class VerifyOtpDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Purpose { get; set; }
        public string Code { get; set; }
    }

}
