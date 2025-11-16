using System.Security.Claims;
using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    private int GetUserIdFromToken()
    {
        return int.Parse(User.FindFirst("userId").Value);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllReturns()
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

    [HttpGet("{returnId}")]
    [Authorize(Roles = "admin,client")]
    public async Task<IActionResult> GetReturnById(int returnId)
    {
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
            RequestedAt = r.RequestedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "client")]
    public async Task<IActionResult> RequestReturn([FromBody] ReturnDto.CreateReturnDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserIdFromToken();

        var existing = await _returnService.GetReturnsByOrderItemIdAsync(dto.OrderItemId);
        if (existing != null)
            return Conflict("A return request already exists for this order item.");

        var newReturn = await _returnService.RequestReturnAsync(userId, dto.OrderItemId, dto.Reason);

        return CreatedAtAction(nameof(GetReturnById), new { returnId = newReturn.ReturnID },
            new ReturnDto.ReturnResponseDto
            {
                ReturnID = newReturn.ReturnID,
                OrderItemID = newReturn.OrderItemID,
                Reason = newReturn.Reason,
                Status = newReturn.Status,
                RequestedAt = newReturn.RequestedAt
            });
    }

    [HttpPut("{returnId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateReturnStatus(int returnId, [FromBody] ReturnDto.UpdateReturnStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _returnService.UpdateReturnStatusAsync(returnId, dto.Status);
        if (!success)
            return NotFound("Return not found.");

        return Ok(new { Message = "Status updated successfully." });
    }

    [HttpDelete("{returnId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteReturn(int returnId)
    {
        var success = await _returnService.DeleteReturnAsync(returnId);
        if (!success)
            return NotFound("Return not found.");

        return Ok(new { Message = "Return deleted successfully." });
    }

    [HttpPut("{returnId}/cancel")]
    [Authorize(Roles = "admin,client")]
    public async Task<IActionResult> CancelReturn(int returnId)
    {
        var r = await _returnService.GetReturnByIdAsync(returnId);
        if (r == null)
            return NotFound("Return not found.");

        bool isAdmin = User.IsInRole("admin");

        if (!isAdmin && r.UserId != GetUserIdFromToken())
            return Forbid("You cannot cancel another user's return.");

        if (r.Status == "approved")
            return BadRequest("Cannot cancel an approved return.");

        if (r.Status == "rejected")
            return BadRequest("Cannot cancel a rejected return.");

        if (r.IsCancelled)
            return BadRequest("Return is already cancelled.");

        var success = await _returnService.CancelReturnAsync(returnId);
        if (!success)
            return StatusCode(500, "Failed to cancel return.");

        return Ok(new { Message = "Return cancelled successfully." });
    }
    [HttpGet("User")]
    public async Task<IActionResult> GetMyReturnRequests()
    {
        var userId = GetUserIdFromToken();

        var returns = await _returnService.GetReturnRequestsByUserIdAsync(userId);
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
}
