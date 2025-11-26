using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepiFinalProject.core.DTOs
{
    public class CreateUserDTO
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
        public string UserRole { get; set; } = "client";

    }
}
