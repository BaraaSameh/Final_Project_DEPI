using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage ="User id is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "CurrentPassword is required")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "NewPassword is required")]

        public string NewPassword { get; set; }
    }
}
