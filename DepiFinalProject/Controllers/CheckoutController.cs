using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        public PaymentsController(IPaymentRepository paymentRepository,
             IOrderService orderService, 
             IPaymentService paymentService, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
            _orderService = orderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a new PayPal payment for an order
        /// Amount is automatically retrieved from the order
        /// </summary>
        /// <param name="dto">Only OrderId is required</param>
        /// <returns>Payment details + PayPal approval URL</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            //if (dto == null || dto.Amount <= 0)
            //    return BadRequest(new { message = "Invalid payment amount" });

            //try
            //{
            //    var (paymentId, approveUrl) = await _paymentService.CreateOrderAsync(dto.Amount, dto.OrderID);

            //    var payment = await _paymentRepository.GetByIdAsync(paymentId);

            //    if (payment == null)
            //        return BadRequest(new { message = "Payment could not be created" });

            //    var response = MapPaymentToDto(payment, approveUrl);

            //    return Ok(new
            //    {
            //        payment = response,
            //        approveUrl,
            //        message = "PayPal order created successfully"
            //    });
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500,  $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            //}
            try
            {
                // Get the order first to retrieve its amount
                var order = await _orderService.GetByIdAsync(dto.OrderID);

                if (order == null)
                    return NotFound(new { message = $"Order with ID {dto.OrderID} not found" });

                // Verify order belongs to current user (security check)
                var userIdClaim = User.FindFirst("userId")?.Value;
                var isAdmin = User.IsInRole("admin");

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int currentUserId))
                {
                    if (order.UserID != currentUserId && !isAdmin)
                        return Forbid();
                }

                // Check if order is in valid state for payment
                if (order.OrderStatus == "Cancelled")
                    return BadRequest(new { message = "Cannot create payment for cancelled order" });

                if (order.OrderStatus == "Delivered")
                    return BadRequest(new { message = "Order already completed" });

                // Get amount automatically from order
                decimal amount = order.TotalAmount;

                // Create payment with order's amount
                var (paymentId, approveUrl) = await _paymentService.CreateOrderAsync(amount, dto.OrderID);

                var payment = await _paymentRepository.GetByIdAsync(paymentId);

                if (payment == null)
                    return BadRequest(new { message = "Payment could not be created" });

                var response = MapPaymentToDto(payment, approveUrl);

                return Ok(new
                {
                    payment = response,
                    approveUrl,
                    message = $"PayPal order created successfully for amount ${amount}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating payment.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get all payments (admin only)
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPayments()
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin " });
            }

            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                var paymentDtos = payments.Select(p => MapPaymentToDto(p)).ToList(); // explicit lambda
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get payment by ID (admin or the payment owner)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentById(string id)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                    return NotFound(new { message = "Payment not found" });

                var userId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("admin");

                if (payment.UserId != userId && !isAdmin)
                    return Forbid();

                return Ok(MapPaymentToDto(payment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get all payments for a specific user (admin or the user themselves)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments(int userId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("admin");

                if (currentUserId != userId && !isAdmin)
                    return Forbid();

                var payments = await _paymentRepository.GetByUserIdAsync(userId);
                var paymentDtos = payments.Select(p => MapPaymentToDto(p)).ToList(); // explicit lambda
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update payment status (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentDto dto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                    return NotFound(new { message = "Payment not found" });

                if (!string.IsNullOrEmpty(dto.Status))
                    payment.Status = dto.Status;

                if (!string.IsNullOrEmpty(dto.PaymentMethod))
                    payment.PaymentMethod = dto.PaymentMethod;

                await _paymentRepository.UpdateAsync(payment);

                return Ok(new
                {
                    message = "Payment updated successfully",
                    payment = MapPaymentToDto(payment)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Maps Payment entity to PaymentResponseDto
        /// </summary>
        /// <param name="payment">Payment entity</param>
        /// <param name="approveUrl">Optional PayPal approval URL</param>
        /// <returns>PaymentResponseDto</returns>
        private PaymentResponseDto MapPaymentToDto(Payment payment, string approveUrl = null)
        {
            return new PaymentResponseDto
            {
                PaymentID = payment.PaymentID,
                OrderID = payment.OrderID ?? 0,
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                PaidAt = payment.PaidAt,

                Order = payment.Order == null ? null : new OrderSummaryDto
                {
                    OrderNumber = payment.Order.OrderNo,
                    TotalAmount = payment.Order.TotalAmount
                },

                User = payment.User == null ? null : new UserSummaryDto
                {
                    UserId = payment.User.UserID,
                    Email = payment.User.UserEmail
                },

                PaymentDetails = new PaymentDetailsDto
                {
                    PayPalOrderId = payment.PayPalOrderId,
                    PayPalLink = approveUrl ??
                        (string.IsNullOrEmpty(payment.PayPalOrderId) ? "" : _configuration["PayPal:SandboxApproveBase"] + payment.PayPalOrderId)
                }
            };
        }
    }
}
