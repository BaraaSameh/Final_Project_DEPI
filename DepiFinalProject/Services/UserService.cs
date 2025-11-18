using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;

namespace DepiFinalProject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ICloudinaryService _cloudinary;
        public UserService(IUserRepository userRepository, IPasswordService passwordService, ICloudinaryService cloudinary)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _cloudinary = cloudinary;
        }
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            if (!_passwordService.VerifyPassword(currentPassword, user.UserPassword))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            user.UserPassword = _passwordService.HashPassword(newPassword);
            return await _userRepository.ChangePasswordAsync(user);
        }

        public async Task<User> CreateAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.UserPassword))
            {
                user.UserPassword = _passwordService.HashPassword(user.UserPassword);
            }
            user.CreatedAt = DateTime.UtcNow;
            return await _userRepository.CreateAsync(user);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<ICollection<UserResponseDTO>> GetAllAsync()
        {
            var users= await _userRepository.GetAllAsync();
            var usersResponse = users.Select(user => new UserResponseDTO
            {
                UserID = user.UserID,
                UserEmail = user.UserEmail,
                UserName = user.UserFirstName + " " + user.UserLastName,
                UserRole = user.UserRole,
                AddressNumber = user.Addresses?.Count ?? 0,
                CartsNumber = user.Carts?.Count ?? 0,
                OrdersNumber = user.Orders?.Count ?? 0,
                ReviewsNumber = user.Reviews?.Count ?? 0,
                WishListNumber = user.Wishlists?.Count ?? 0,
                imgeurl = user.ImageUrl ?? string.Empty,
                imageid = user.ImagePublicId ?? string.Empty,
            }).ToList();
            return usersResponse;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<UserResponseDTO> GetByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return new UserResponseDTO
            {
                UserID = user.UserID,
                UserEmail= user.UserEmail,
                UserName= user.UserFirstName +" "+ user.UserLastName,
                UserRole = user.UserRole,
                AddressNumber = user.Addresses?.Count ?? 0,
                CartsNumber = user.Carts?.Count ?? 0,
                OrdersNumber = user.Orders?.Count ?? 0,
                ReviewsNumber = user.Reviews?.Count ?? 0,
                WishListNumber = user.Wishlists?.Count ?? 0,
                imgeurl = user.ImageUrl ?? string.Empty,
                imageid=user.ImagePublicId??string.Empty,
            };
        }

        public async Task<User> UpdateAsync(User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(user.UserID);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {user.UserID} not found");
            }

            existingUser.UserFirstName = user.UserFirstName;
            existingUser.UserLastName = user.UserLastName;
            existingUser.UserPhone = user.UserPhone;
            existingUser.UserEmail = user.UserEmail;
            existingUser.UserRole = user.UserRole;
            

            return await _userRepository.UpdateAsync(existingUser);
        }
        public async Task<string> UpdateUserImageAsync(int userId, IFormFile file)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {user.UserID} not found");

            if (!string.IsNullOrEmpty(user.ImagePublicId))
                await _cloudinary.DeleteImageAsync(user.ImagePublicId);
            

            var (url, publicId) = await _cloudinary.UploadUserImageAsync(file);

            user.ImageUrl = url;
            user.ImagePublicId = publicId;

            await _userRepository.UpdateAsync(user);

            return url;
        }

        public async Task<bool> DeleteUserImageAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {user.UserID} not found");

            if (string.IsNullOrEmpty(user.ImagePublicId))
                return false;

            var deleted = await _cloudinary.DeleteImageAsync(user.ImagePublicId);

            if (deleted)
            {
                user.ImageUrl = null;
                user.ImagePublicId = null;
                await _userRepository.UpdateAsync(user);
            }
            

                return deleted;
        }
    }

}

