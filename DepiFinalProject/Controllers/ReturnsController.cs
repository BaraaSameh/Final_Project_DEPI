using DepiFinalProject.core.Models;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;
    private readonly ReturnSettings _returnSettings;


    public ReturnsController(
        IReturnService returnService, 
        Microsoft.Extensions.Options.IOptions<ReturnSettings> returnSettings)

    {
        _returnService = returnService;
        _returnSettings = returnSettings.Value;
    }

    private int GetUserIdFromToken()
    {
        return int.Parse(User.FindFirst("userId").Value);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ReturnDto.ReturnResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllReturns()
    {
        if (!User.IsInRole("admin") && !User.IsInRole("seller"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
        }

        var returns = await _returnService.GetAllReturnsAsync();

        var result = returns.Select(r => new ReturnDto.ReturnResponseDto
        {
            ReturnID = r.ReturnID,
            OrderItemID = r.OrderItemID,
            Reason = r.Reason,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            RefundStatus = r.RefundStatus,
            RefundAmount = r.RefundAmount,
            RefundedAt = r.RefundedAt
        });

        return Ok(result);
    }


    [HttpGet("{returnId}")]
    [Authorize]
    [ProducesResponseType(typeof(ReturnDto.ReturnResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnById(int returnId)
    {
        if (!User.IsInRole("admin") && !User.IsInRole("seller") && !User.IsInRole("client"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin, Seller And Client" });
        }

        var r = await _returnService.GetReturnByIdAsync(returnId);
        if (r == null)
            return NotFound(new { Message = "Return not found." });

        if (User.IsInRole("client") && r.UserId != GetUserIdFromToken())
            return Forbid("You cannot view another user's return request.");

        return Ok(new ReturnDto.ReturnResponseDto
        {
            ReturnID = r.ReturnID,
            OrderItemID = r.OrderItemID,
            Reason = r.Reason,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            RefundStatus = r.RefundStatus,
            RefundAmount = r.RefundAmount,
            RefundedAt = r.RefundedAt
        });
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReturnDto.ReturnResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestReturn([FromBody] ReturnDto.CreateReturnDto dto)
    {
        if (!User.IsInRole("admin") && !User.IsInRole("client"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserIdFromToken();

            var existing = await _returnService.GetReturnsByOrderItemIdAsync(dto.OrderItemId);
            if (existing != null)
                return Conflict(new { message = "A return request already exists for this order item." });

            var newReturn = await _returnService.RequestReturnAsync(userId, dto.OrderItemId, dto.Reason);

            return CreatedAtAction(nameof(GetReturnById), new { returnId = newReturn.ReturnID },
                new ReturnDto.ReturnResponseDto
                {
                    ReturnID = newReturn.ReturnID,
                    OrderItemID = newReturn.OrderItemID,
                    Reason = newReturn.Reason,
                    Status = newReturn.Status,
                    RequestedAt = newReturn.RequestedAt,
                    RefundStatus = newReturn.RefundStatus
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your return request.", error = ex.Message });
        }
    }

    [HttpPut("{returnId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReturnStatus(int returnId, [FromBody] ReturnDto.UpdateReturnStatusDto dto)
    {
        if (!User.IsInRole("admin") && !User.IsInRole("seller"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _returnService.UpdateReturnStatusAsync(returnId, dto.Status);
            if (!success)
                return NotFound(new { message = "Return not found." });

            var updatedReturn = await _returnService.GetReturnByIdAsync(returnId);

            return Ok(new
            {
                Message = "Status updated successfully.",
                Return = new ReturnDto.ReturnResponseDto
                {
                    ReturnID = updatedReturn.ReturnID,
                    OrderItemID = updatedReturn.OrderItemID,
                    Reason = updatedReturn.Reason,
                    Status = updatedReturn.Status,
                    RequestedAt = updatedReturn.RequestedAt,
                    RefundStatus = updatedReturn.RefundStatus,
                    RefundAmount = updatedReturn.RefundAmount,
                    RefundedAt = updatedReturn.RefundedAt
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the return status.", error = ex.Message });
        }
    }

    [HttpPost("{returnId}/refund")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessRefund(int returnId)
    {
        if (!User.IsInRole("admin"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin" });
        }

        try
        {
            var returnRequest = await _returnService.ProcessRefundAsync(returnId);

            return Ok(new
            {
                Message = "Refund processed successfully.",
                Return = new ReturnDto.ReturnResponseDto
                {
                    ReturnID = returnRequest.ReturnID,
                    OrderItemID = returnRequest.OrderItemID,
                    Reason = returnRequest.Reason,
                    Status = returnRequest.Status,
                    RequestedAt = returnRequest.RequestedAt,
                    RefundStatus = returnRequest.RefundStatus,
                    RefundAmount = returnRequest.RefundAmount,
                    RefundedAt = returnRequest.RefundedAt
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to process refund.", error = ex.Message });
        }
    }

    [HttpDelete("{returnId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturn(int returnId)
    {
        if (!User.IsInRole("admin"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin" });
        }

        var success = await _returnService.DeleteReturnAsync(returnId);
        if (!success)
            return NotFound(new { message = "Return not found." });

        return Ok(new { Message = "Return deleted successfully." });
    }

    [HttpPut("{returnId}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelReturn(int returnId)
    {
        if (!User.IsInRole("admin") && !User.IsInRole("client"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
        }

        try
        {
            var r = await _returnService.GetReturnByIdAsync(returnId);
            if (r == null)
                return NotFound(new { message = "Return not found." });

            bool isAdmin = User.IsInRole("admin");

            if (!isAdmin && r.UserId != GetUserIdFromToken())
                return Forbid("You cannot cancel another user's return.");

            if (r.Status == "approved")
                return BadRequest(new { message = "Cannot cancel an approved return." });

            if (r.Status == "rejected")
                return BadRequest(new { message = "Cannot cancel a rejected return." });

            if (r.IsCancelled)
                return BadRequest(new { message = "Return is already cancelled." });

            var success = await _returnService.CancelReturnAsync(returnId);
            if (!success)
                return StatusCode(500, new { message = "Failed to cancel return." });

            return Ok(new { Message = "Return cancelled successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while cancelling the return.", error = ex.Message });
        }
    }

    [HttpGet("User")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ReturnDto.ReturnResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyReturnRequests()
    {
        if (!User.IsInRole("admin") && !User.IsInRole("client"))
        {
            return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
        }

        var userId = GetUserIdFromToken();

        var returns = await _returnService.GetReturnRequestsByUserIdAsync(userId);
        var result = returns.Select(r => new ReturnDto.ReturnResponseDto
        {
            ReturnID = r.ReturnID,
            OrderItemID = r.OrderItemID,
            Reason = r.Reason,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            RefundStatus = r.RefundStatus,
            RefundAmount = r.RefundAmount,
            RefundedAt = r.RefundedAt
        });

        return Ok(result);
    }

    [HttpGet("config")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetReturnConfig()
    {
        return Ok(new
        {
            ReturnWindowDays = _returnSettings.ReturnWindowDays,
            AllowedReturnReasons = _returnSettings.AllowedReturnReasons,
            AllowedReturnStatuses = _returnSettings.AllowedReturnStatuses,
            RefundProcessingNote = _returnSettings.RefundProcessingNote
        });
    }
}
