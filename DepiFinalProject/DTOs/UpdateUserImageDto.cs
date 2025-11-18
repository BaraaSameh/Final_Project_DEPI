using Microsoft.AspNetCore.Http;
namespace DepiFinalProject.DTOs
{   
        public class UpdateUserImageDto
        {
            public IFormFile Image { get; set; }
        }

}
