﻿using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }
        public string Description { get; set; }

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }
    }
}
