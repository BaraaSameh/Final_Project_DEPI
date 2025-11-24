using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DepiFinalProject.DTOs.AddressDTO;

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
        /// Get all addresses for a user
        /// </summary>
        [HttpGet("{userId}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(IEnumerable<AddressDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetUserAddresses(int userId)
        {
            try
            {
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
        /// Add a new address
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(AddressDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<AddressDto>> AddAddress([FromBody] AddressCreateUpdateDto addressDto)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
        /// Update an existing address
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AddressDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] AddressCreateUpdateDto addressDto)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
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
        /// Delete an address
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
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
    }
}
