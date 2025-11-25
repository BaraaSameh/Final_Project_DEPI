using System.Security.Claims;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAddressService _addressService;
        public UserController(IUserService userService, IAddressService addressService)
        {
            _userService = userService;
            _addressService = addressService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ICollection<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ICollection<User>>> GetAllUsers()
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            try
            {
               
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = $"User with email {email} not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user", error = ex.Message });
            }
        }
        /// <summary>
        /// Upload or update the user's profile image.
        /// </summary>
        /// <remarks>
        /// Accepted formats: JPG, PNG, WEBP  
        /// Max recommended size: 1MB  
        /// Returns the Cloudinary secure URL.
        /// </remarks>
        /// <param name="id">User ID</param>
        /// <param name="dto">Image file</param>
        /// <response code="200">Image uploaded successfully</response>
        /// <response code="400">Invalid image or validation error</response>
        /// <response code="401">User not registerd error</response>
        [HttpPost("/upload-image")]
        [Authorize]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage([FromForm] UpdateUserImageDto dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            if (!TryGetUserId(out var userid)) Unauthorized("Only registered Users can upload thier images");
            var url = await _userService.UpdateUserImageAsync(userid, dto.Image);
            return Ok(new { imageUrl = url });
        }

        /// <summary>
        /// Delete user's profile image.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <response code="200">Image deleted successfully</response>
        /// <response code="404">User or image not found</response>
        /// <response code="401">User not registerd error</response>
        /// <reponse code="404">NO User Image Found</reponse>
        [HttpDelete("/delete-image")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeleteImage()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userid)) Unauthorized("Only registered Users can upload thier images");

            var deleted = await _userService.DeleteUserImageAsync(userid);

            if (!deleted)
                return NotFound("No image found to delete.");

            return Ok(new { message = "Image deleted successfully" });
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _userService.EmailExistsAsync(user.UserEmail))
                {
                    return Conflict(new { message = "Email already exists" });
                }

                var createdUser = await _userService.CreateAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserID }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserDTO user)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loggedUserId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("admin");

                if (id != loggedUserId && !isAdmin)
                    return Unauthorized(new { message = "User ID mismatch" });

                var userdto = await _userService.GetByIdAsync(id);
                var userdata = await _userService.GetByEmailAsync(userdto.UserEmail);
                if (userdata == null)
                    return NotFound();

                userdata.UserEmail = user.UserEmail;
                userdata.UserPhone = user.UserPhone;
                userdata.UserFirstName = user.UserFirstName;
                userdata.UserLastName = user.UserLastName;
                
                var updatedUser = await _userService.UpdateAsync(userdata);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the user",
                    error = ex.Message
                });
            }
        }
        [HttpPut("updateRole/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> UpdateUserRole(int id, [FromBody] UpdateUserRoleDTO user) {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin " });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var userId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("admin");
                var userdata = await _userService.GetByIdAsync(id);


                if (id != userId && !isAdmin)
                {
                    return Unauthorized(new { message = "User ID mismatch" });
                }
                var olduser = await _userService.GetByEmailAsync(userdata.UserEmail);
                olduser.UserRole = user.UserRole;


                var updatedUser = await _userService.UpdateAsync(olduser);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user", error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            try
            {
                var result = await _userService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing the password", error = ex.Message });
            }
        }

        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking email", error = ex.Message });
            }
        }
        private bool TryGetUserId(out int userId)
        {
            userId = default;
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out userId);
        }
       

    }

}
