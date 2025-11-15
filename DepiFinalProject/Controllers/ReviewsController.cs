using DepiFinalProject.DTOs.Reviews;
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            try {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(reviews);
            }
            catch(Exception ex) {
                return StatusCode(500, new { message = "An error occurred while retrieving reviews", error = ex.Message });
            }

        }

        [HttpPost]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto dto)
        {
            try {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var created = await _reviewService.AddReviewAsync(userId, dto);
                return Ok(created);
            }
            catch(Exception ex) {
                return BadRequest(new { message = "Invalid review data", error = ex.Message });
            }
            
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,client,seller")]

        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewUpdateDto dto)
        {
            try {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var isAdmin = User.IsInRole("Admin");
                var updated = await _reviewService.UpdateReviewAsync(id, userId, dto, isAdmin);
                return Ok(updated);
            }
            catch(Exception ex) {
                return BadRequest(new { message = "Invalid review data", error = ex.Message });
            }
          
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var isAdmin = User.IsInRole("Admin");
                var result = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);
                if (!result) return NotFound();
                return Ok("Deleted successfully");
            }
            catch(Exception ex) {
                return BadRequest(new { message = "Error deleting review", error = ex.Message });
            }
           
        }
    }
}
