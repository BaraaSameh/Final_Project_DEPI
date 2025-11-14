using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        // GET: api/addresses/{userId}
        [HttpGet("{userId}")]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetUserAddresses(int userId)
        {
            try
            {
                

                var addresses = await _addressService.GetUserAddressesAsync(userId);

               
                if (addresses == null || !addresses.Any())
                {
                    return Ok(new List<AddressDto>());
                }

                
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
                
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving addresses", error = ex.Message });
            }
        }

        // POST: api/addresses
        [HttpPost]
        [Authorize(Roles = "admin,client")]

        public async Task<ActionResult<AddressDto>> AddAddress([FromBody] AddressCreateUpdateDto addressDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = new Address
                {
                    // 💡 يجب التأكد من أن AddressCreateUpdateDto يحتوي على UserID
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
                
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the address", error = ex.Message });
            }

        }

        // PUT: api/addresses/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,client")]

        public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] AddressCreateUpdateDto addressDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
         
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the address", error = ex.Message });
            }
        }

        // DELETE: api/addresses/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,client")]

        public async Task<ActionResult> DeleteAddress(int id)
        {
            try
            {
             
                var result = await _addressService.DeleteAddressAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the address", error = ex.Message });
            }

        }
    }
}


    