using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Core.DTOs
{
    public class AuthenticationDTO
    {
        //request DTOs

        public class RegisterRequest
        {
            [Required(ErrorMessage = "User email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [RegularExpression(@"^[A-Za-z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com|hotmail\.com|icloud\.com)$",
                ErrorMessage = "Email must be from a known provider (gmail, yahoo, outlook, hotmail, icloud)")]
            public string UserEmail { get; set; }

            [Required(ErrorMessage = "User password is required")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};:'""\\|,.<>\/?]).{6,}$",
                ErrorMessage = "Password must contain an uppercase letter, a number, and a special character")]
            public string UserPassword { get; set; }

            [Required(ErrorMessage = "First name is required")]
            [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "First name must contain only letters")]
            public string UserFirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Last name must contain only letters")]
            public string UserLastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^(010|011|012|015)[0-9]{8}$",
                ErrorMessage = "Phone number must be a valid Egyptian mobile number (e.g., 010xxxxxxxx)")]
            public string UserPhone { get; set; }
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "User email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [RegularExpression(@"^[A-Za-z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com|hotmail\.com|icloud\.com)$",
                ErrorMessage = "Email must be from a known provider")]
            public string UserEmail { get; set; }

            [Required(ErrorMessage = "User password is required")]
            public string UserPassword { get; set; }
        }

        public class RefreshTokenRequest
        {
            [Required(ErrorMessage = "token is required")]
            public string RefreshToken { get; set; }
        }

        public class RevokeTokenRequest
        {
            [Required(ErrorMessage = "token is required")]
            public string RefreshToken { get; set; }
        }

        //response DTOs

        public class AuthenticationResponse
        {
            //public int UserId { get; set; }
            //public string UserEmail { get; set; }
            //public string UserFirstName { get; set; }
            //public string UserLastName { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
