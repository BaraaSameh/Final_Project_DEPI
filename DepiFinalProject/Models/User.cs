﻿using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DepiFinalProject.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        public string UserPassword { get; set; }  // هاش الباسوورد في الواقع

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserRole { get; set; }  // مثل "Admin" أو "Customer"

        // Navigation Properties
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
