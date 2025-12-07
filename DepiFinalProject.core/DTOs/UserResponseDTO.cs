using DepiFinalProject.Core.Models;
using static DepiFinalProject.Core.DTOs.AddressDTO;

namespace DepiFinalProject.Core.DTOs
{
    public class UserResponseDTO
    {
        public int UserID { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string userFirstName { get; set; }
        public string userlastName { get; set; }
        public string phone { get; set; }
        public string UserRole { get; set; } = "";
        public int AddressNumber { get; set; }
        public int CartsNumber { get; set; }
        public int OrdersNumber { get; set; }
        public int ReviewsNumber { get; set; }
        public int WishListNumber { get; set; }
     
        public string imgeurl { get; set; }
        public string imageid { get; set; }
    }
    public class UpdateUserDTO {
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }

     }
    public class UpdateUserRoleDTO {
        public string UserRole { get; set; }
    }
}
