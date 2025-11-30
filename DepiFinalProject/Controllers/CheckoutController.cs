using System.Security.Claims;
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
        private readonly IInvoiceTokenService _tokenService;
        private readonly IUserService _userService;
        public PaymentsController(IPaymentRepository paymentRepository,
             IOrderService orderService,
             IPaymentService paymentService, Microsoft.Extensions.Configuration.IConfiguration configuration, IInvoiceTokenService tokenService, IUserService userService)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
            _orderService = orderService;
            _configuration = configuration;
            _tokenService = tokenService;
            _userService = userService;
        }

        /// <summary>
        /// Creates a new PayPal payment for an order.
        /// </summary>
        /// <remarks>
        /// The order amount is automatically retrieved based on the OrderID.
        /// Only admins and clients can create payments.
        /// </remarks>
        /// <param name="dto">Object containing only the OrderID.</param>
        /// <response code="200">Payment created successfully.</response>
        /// <response code="400">Invalid request or order not eligible for payment.</response>
        /// <response code="403">Unauthorized to create payment.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                if(order.OrderStatus.ToLower()== "paid")
                    return BadRequest("Order is already paid");
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
        /// Retrieves all payments in the system.
        /// </summary>
        /// <remarks>Only admins can view all payments.</remarks>
        /// <response code="200">Returns all payments.</response>
        /// <response code="403">User is not admin.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// Retrieves a specific payment by its ID.
        /// </summary>
        /// <remarks>
        /// Only admins or the payment's owner can access this.
        /// </remarks>
        /// <param name="id">Payment ID.</param>
        /// <response code="200">Payment returned successfully.</response>
        /// <response code="403">Not authorized to access this payment.</response>
        /// <response code="404">Payment not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Captures a PayPal order after the user approves payment.
        /// </summary>
        /// <param name="paypalOrderId">PayPal order ID (token returned from PayPal)</param>
        [HttpPost("capture")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CapturePayment([FromQuery] string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return BadRequest(new { message = "Missing PayPal order ID" });

            try
            {
                var result = await _paymentService.CaptureOrderAsync(orderId);

                return Ok(new
                {
                    message = "Payment captured successfully",
                    status = result.Status.ToString(),
                    paypalOrderId = orderId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error capturing PayPal order",
                    details = ex.Message
                });
            }
        }
        /// <summary>
        /// Captures a PayPal order after the user approves payment.
        /// </summary>
        /// <param name="paypalOrderId">PayPal order ID (token returned from PayPal)</param>
        [HttpGet("cancel")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> OnCancel(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return BadRequest(new { message = "Missing PayPal order ID" });

            try
            {
                var result = await _paymentService.CancelPayment(orderId);

                return Ok(new
                {
                    message = "Payment Canceld successfully",
                    status = result.Status.ToString(),
                    paypalOrderId = orderId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error capturing PayPal order",
                    details = ex.Message
                });
            }
        }


        /// <summary>
        /// Retrieves all payments made by a specific user.
        /// </summary>
        /// <remarks>Accessible by admin or the user themselves.</remarks>
        /// <param >User ID.</param>
        /// <response code="200">Payments retrieved successfully.</response>
        /// <response code="403">User not authorized to access these payments.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("user")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments()
        {
            if (!TryGetUserId(out int userId)) return Unauthorized("Allowed Only for authorized users");
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
        /// Retrieves invoice summary data for a specific payment using a secure invoice token.
        /// </summary>
        /// <remarks>
        /// This endpoint is accessible without authentication because the token itself is secure
        /// and single-use.  
        /// Used for generating public invoice pages that users can open from email links.
        /// </remarks>
        /// <param name="token">A secure, single-use token that identifies a payment.</param>
        /// <response code="200">Invoice data retrieved successfully.</response>
        /// <response code="401">Invalid or expired token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("invoice")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetInvoiceData([FromQuery] string token)
        {
            var payment = _tokenService.ValidateAndConsumeToken(token);
            if (payment == null) return Unauthorized();

            var userd = await _userService.GetByIdAsync(payment.UserId);
            var user = await _userService.GetByEmailAsync(userd.UserEmail);
            var order = await _orderService.GetByIdAsync(payment.OrderID!.Value);

            var data = new
            {
                paymentId = payment.PaymentID,
                paidAt = payment.PaidAt,
                customerName = $"{user.UserFirstName} {user.UserLastName}".Trim(),
                customerEmail = user.UserEmail,
                orderNo = order.OrderNo,
                amount = payment.Amount.ToString("F2")
            };

            return Ok(data);
        }
        /// <summary>
        /// Updates the status or method of a payment.
        /// </summary>
        /// <remarks>Only admins can update payment status.</remarks>
        /// <param name="id">Payment ID.</param>
        /// <param name="dto">New payment status or method.</param>
        /// <response code="200">Payment updated successfully.</response>
        /// <response code="403">Unauthorized access.</response>
        /// <response code="404">Payment not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        private bool TryGetUserId(out int userId)
        {
            userId = default;
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out userId);
        }
    }
}

