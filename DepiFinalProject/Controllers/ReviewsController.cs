using DepiFinalProject.DTOs.Reviews;
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Specialized;
using System.Security.Claims;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Get all reviews for a specific product.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of reviews for the product</returns>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReviewResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviewsByProductId(int productId)
        {
            var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
            return Ok(reviews);
        }

        /// <summary>
        /// Get a single review by id.
        /// </summary>
        [HttpGet("{id}", Name = "GetReviewById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewResponseDto>> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewsByProductIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        /// <summary>
        /// Add a new review.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReviewResponseDto>> AddReview([FromBody] ReviewCreateDto dto)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized("only registerd users can add reviews");
            try
            {
                var created = await _reviewService.AddReviewAsync(userId, dto);
                return CreatedAtRoute("GetReviewById", new { id = created.ReviewID }, created);


            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest($"{ex.Message}");
            }

        }

        /// <summary>
        /// Update an existing review.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReviewResponseDto>> UpdateReview(int id, [FromBody] ReviewUpdateDto dto)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized("You can only update Reviews that you own!");
            var isAdmin = User.IsInRole("admin");

            var updated = await _reviewService.UpdateReviewAsync(id, userId, dto, isAdmin);
            if (updated == null)
                return NotFound(new { Message = $"Review with ID {id} not found or not authorized." });

            return Ok(updated);
        }

        /// <summary>
        /// Delete a review.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized("You can only Delte Reviews that you own!");
            var isAdmin = User.IsInRole("admin");

            var deleted = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);
            if (!deleted) return NotFound(new { Message = $"Review with ID {id} not found or not authorized." });

            return NoContent();
        }

        /// <summary>
        /// Get all reviews of this user.
        /// </summary>
        [HttpGet("/User")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReviewResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> getreviewsByUsers()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
           
            return Ok(reviews);
        }

        private bool TryGetUserId(out int userId)
        {
            userId = default;
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out userId);
        }
    }
}
