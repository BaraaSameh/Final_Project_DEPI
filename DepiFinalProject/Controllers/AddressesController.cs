using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DepiFinalProject.Core.DTOs.AddressDTO;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Retrieves all saved addresses for a specific user.
        /// </summary>
        /// <param>User ID.</param>
        /// <returns>A list of addresses belonging to the user.</returns>
        /// <response code="200">Returns the list of addresses.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpGet("user")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetUserAddresses()
        {
            try
            {
                if (!TryGetUserId(out int userId)) return Unauthorized("Allowed Only for authorized users");
                if (userId <= 0)
                    return BadRequest(new { Message = "Invalid user ID." });

                var addresses = await _addressService.GetUserAddressesAsync(userId);

                if (addresses == null || !addresses.Any())
                    return Ok(new List<AddressDto>()); // Always return list

                var addressDtos = addresses.Select(a => new AddressDto
                {
                    AddressID = a.AddressID,
                    UserID = a.UserID,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    Country = a.Country
                }).ToList();

                return Ok(addressDtos);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving addresses",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Creates a new address for a user.
        /// </summary>
        /// <param name="addressDto">Address details.</param>
        /// <returns>The created address object.</returns>
        /// <response code="201">Address successfully created.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddressDto>> AddAddress([FromBody] AddressCreateUpdateDto addressDto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (addressDto == null)
                    return BadRequest(new { Message = "Address data is required." });

                var currentUserId = GetUserId();
                if (User.IsInRole("client") && currentUserId != addressDto.UserID)
                    return Forbid("You are not authorized to add addresses for this user.");

                var address = new Address
                {
                    UserID = addressDto.UserID,
                    FullAddress = addressDto.FullAddress,
                    City = addressDto.City,
                    Country = addressDto.Country
                };

                var createdAddress = await _addressService.CreateAddressAsync(address);

                var resultDto = new AddressDto
                {
                    AddressID = createdAddress.AddressID,
                    UserID = createdAddress.UserID,
                    FullAddress = createdAddress.FullAddress,
                    City = createdAddress.City,
                    Country = createdAddress.Country
                };

                return CreatedAtAction(nameof(GetUserAddresses), new { userId = resultDto.UserID }, resultDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while creating the address",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing address.
        /// </summary>
        /// <param name="id">Address ID.</param>
        /// <param name="addressDto">Address details to update.</param>
        /// <returns>The updated address information.</returns>
        /// <response code="200">Address updated successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="404">Address not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] AddressCreateUpdateDto addressDto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "Invalid address ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = GetUserId();
                if (User.IsInRole("client") && currentUserId != addressDto.UserID)
                    return Forbid("You are not authorized to update this address.");

                var address = new Address
                {
                    UserID = addressDto.UserID,
                    FullAddress = addressDto.FullAddress,
                    City = addressDto.City,
                    Country = addressDto.Country
                };

                var updatedAddress = await _addressService.UpdateAddressAsync(id, address);

                var resultDto = new AddressDto
                {
                    AddressID = updatedAddress.AddressID,
                    UserID = updatedAddress.UserID,
                    FullAddress = updatedAddress.FullAddress,
                    City = updatedAddress.City,
                    Country = updatedAddress.Country
                };

                return Ok(resultDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while updating the address",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Deletes an address by ID.
        /// </summary>
        /// <param name="id">Address ID.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Address deleted successfully.</response>
        /// <response code="400">Invalid address ID.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="404">Address not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "Invalid address ID." });
               
                var currentUserId = GetUserId();
                var address = await _addressService.GetAddressByIdAsync(id);

                if (address == null)
                    return NotFound(new { Message = $"Address with ID {id} not found." });

                if (User.IsInRole("client") && currentUserId != address.UserID)
                    return Forbid("You are not authorized to delete this address.");

                await _addressService.DeleteAddressAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while deleting the address",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
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
