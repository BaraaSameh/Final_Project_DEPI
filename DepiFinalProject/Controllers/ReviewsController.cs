using DepiFinalProject.Core.DTOs.Reviews;
using DepiFinalProject.Core.Interfaces;
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
        /// <response code="200">Returns list of product reviews.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReviewResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviewsByProductId(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(reviews);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving reviews.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get a review by ID.
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <response code="200">Returns the review.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}", Name = "GetReviewById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReviewResponseDto>> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewsByProductIdAsync(id);
                if (review == null) return NotFound();
                return Ok(review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the review.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Add a new review.
        /// </summary>
        /// <param name="dto">Review data</param>
        /// <response code="201">Review created.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="500">Internal server error.</response>
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           

        }

        /// <summary>
        /// Update an existing review.
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="dto">Updated data</param>
        /// <response code="200">Review updated successfully.</response>
        /// <response code="400">Invalid request.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Review not found or unauthorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReviewResponseDto>> UpdateReview(int id, [FromBody] ReviewUpdateDto dto)
        {
            try
            {
                if (!TryGetUserId(out var userId)) return Unauthorized("You can only update Reviews that you own!");
                var isAdmin = User.IsInRole("admin");

                var updated = await _reviewService.UpdateReviewAsync(id, userId, dto, isAdmin);
                if (updated == null)
                    return NotFound(new { Message = $"Review with ID {id} not found or not authorized." });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the review.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a review.
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <response code="204">Review deleted.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Review not found or unauthorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                if (!TryGetUserId(out var userId)) return Unauthorized("You can only Delte Reviews that you own!");
                var isAdmin = User.IsInRole("admin");

                var deleted = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);
                if (!deleted) return NotFound(new { Message = $"Review with ID {id} not found or not authorized." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the review.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get all reviews created by the logged-in user.
        /// </summary>
        /// <response code="200">Returns the user's reviews.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">No reviews found.</response>
        /// <response code="500">Internal server error.</response> /// <summary>
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

            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving reviews.", Details = ex.Message });
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
