namespace DepiFinalProject.DTOs
{
    public class AuthenticationDTO
    {
        //request DTOs

        public class RegisterRequest
        {
            public string UserEmail { get; set; }
            public string UserPassword { get; set; }
            public string UserFirstName { get; set; }
            public string UserLastName { get; set; }
            public string UserPhone { get; set; }
        }

        public class LoginRequest
        {
            public string UserEmail { get; set; }
            public string UserPassword { get; set; }
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }
        }

        public class RevokeTokenRequest
        {
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
