using Microsoft.AspNetCore.Http;
namespace DepiFinalProject.Core.DTOs
{   
        public class UpdateUserImageDto
        {
            public IFormFile Image { get; set; }
        }

}
