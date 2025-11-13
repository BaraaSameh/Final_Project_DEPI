using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ReturnsController : ControllerBase
    {
        private readonly IReturnService _returnService;

        public ReturnsController(IReturnService returnService)
        {
            _returnService = returnService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAllReturns()
        {
            try
            {
                var returns = await _returnService.GetAllReturnsAsync();
                var result = returns.Select(r => new ReturnDto.ReturnResponseDto
                {
                    ReturnID = r.ReturnID,
                    OrderItemID = r.OrderItemID,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }
        [HttpGet("{returnId}")]
       [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetReturnById(int returnId)
        {
            try
            {
                var returnRequest = await _returnService.GetReturnByIdAsync(returnId);
                if (returnRequest == null)
                    return NotFound($"Return with ID {returnId} not found.");
                var response = new ReturnDto.ReturnResponseDto
                {
                    ReturnID = returnRequest.ReturnID,
                    OrderItemID = returnRequest.OrderItemID,
                    Reason = returnRequest.Reason,
                    Status = returnRequest.Status,
                    RequestedAt = returnRequest.RequestedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }
        [HttpPost]
     [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestReturn([FromBody] ReturnDto.CreateReturnDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var hasany = await _returnService.GetReturnsByOrderItemIdAsync(dto.OrderItemId);
                if (hasany != null) { 
                   
                        return BadRequest("A return request for this order item already exists.");
                    

                }
                var newReturn = await _returnService.RequestReturnAsync(dto.OrderItemId, dto.Reason);
                var response = new ReturnDto.ReturnResponseDto
                {
                    ReturnID = newReturn.ReturnID,
                    OrderItemID = newReturn.OrderItemID,
                    Reason = newReturn.Reason,
                    Status = newReturn.Status,
                    RequestedAt = newReturn.RequestedAt
                };

                return CreatedAtAction(nameof(GetAllReturns), new { id = newReturn.ReturnID }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }

        [HttpPut("{returnId}")]
       [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReturnStatus(int returnId, [FromBody] ReturnDto.UpdateReturnStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _returnService.UpdateReturnStatusAsync(returnId, dto.Status);
                if (!success)
                    return NotFound($"Return with ID {returnId} not found.");

                return Ok($"Return {returnId} status updated to {dto.Status}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{returnId}")]
     [Authorize(Roles = "Admin")]
        async public Task<IActionResult> DeleteReturn(int returnId)
        {
            try
            {
                var success = await _returnService.DeleteReturnAsync(returnId);
                if (!success)
                    return NotFound($"Return with ID {returnId} not found.");
                return Ok($"Return {returnId} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }
        }
        [HttpPut("{returnId}/cancel")]
            [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> CancelReturn(int returnId)
        {
            try
            {
                var returnRequest = await _returnService.GetReturnByIdAsync(returnId);

                if (returnRequest == null)
                {
                    return NotFound(new { Message = "Return request not found." });
                }

                if (returnRequest.Status.ToLower() == "approved")
                {
                    return BadRequest(new { Message = "Cannot cancel return — it has already been approved." });
                }

                if (returnRequest.Status.ToLower() == "rejected")
                {
                    return BadRequest(new { Message = "Cannot cancel return — it has already been rejected." });
                }

                if (returnRequest.Status.ToLower() == "cancelled")
                {
                    return BadRequest(new { Message = "This return request has already been canceled." });
                }

                var success = await _returnService.CancelReturnAsync(returnId);
                if (!success)
                {
                    return StatusCode(500, new { Message = "Failed to cancel the return request." });
                }

                return Ok($"Return {returnId} has been cancelled successfully.");
            }
            catch (Exception ex)
            {
                // optionally log ex.Message
                return StatusCode(500, new { Message = "An unexpected error occurred.", Details = ex.Message });
            }
        }


    }
}
