using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class AuthenticationDTO
    {
        //request DTOs

        public class RegisterRequest
        {
            [Required(ErrorMessage ="User email is required")]
            [EmailAddress]
            public string UserEmail { get; set; }
            [Required(ErrorMessage ="user passowrd is required")]
            [PasswordPropertyText]
            public string UserPassword { get; set; }
            public string UserFirstName { get; set; }
            public string UserLastName { get; set; }
            [Phone]
            [MaxLength(11, ErrorMessage = "Description should be 11 number")]
            [MinLength(11, ErrorMessage = "Description should be 11 number")]
            public string UserPhone { get; set; }
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "User Email is required")]
            [EmailAddress]
            public string UserEmail { get; set; }
            [Required(ErrorMessage ="User Password is required")]
            public string UserPassword { get; set; }
        }

        public class RefreshTokenRequest
        {
            [Required(ErrorMessage = "token is required")]
            public string RefreshToken { get; set; }
        }

        public class RevokeTokenRequest
        {
            [Required(ErrorMessage ="token is required")]
            public string RefreshToken { get; set; }
        }

        //response DTOs

        public class AuthenticationResponse
        {
            public int UserId { get; set; }
            public string UserEmail { get; set; }
            public string UserFirstName { get; set; }
            public string UserLastName { get; set; }
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
