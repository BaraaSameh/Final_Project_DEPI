using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<ICollection<User>>> GetAllUsers()
        {
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
        //[Authorize]
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
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
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

        [HttpPost]
        //[AllowAnonymous]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
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
        //[Authorize]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != user.UserID)
                {
                    return BadRequest(new { message = "User ID mismatch" });
                }

                var updatedUser = await _userService.UpdateAsync(user);
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
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
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
        //[Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
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
        //[AllowAnonymous]
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
    }

}
