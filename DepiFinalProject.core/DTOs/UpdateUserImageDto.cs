using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace DepiFinalProject.Core.DTOs
{   
        public class UpdateUserImageDto
        {
            [Required(ErrorMessage = "Image file is required")]
            public IFormFile Image { get; set; }
        }

}
