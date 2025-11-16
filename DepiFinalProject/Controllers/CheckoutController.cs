using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentService _paymentService;

        private const string PayPalSandboxApproveBase = "https://www.sandbox.paypal.com/checkoutnow?token=";

        public PaymentsController(IPaymentRepository paymentRepository, IPaymentService paymentService)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
        }

        /// <summary>
        /// Create a new PayPal payment (admin, client)
        /// </summary>
        /// <param name="dto">Amount + OrderId</param>
        /// <returns>Payment details + PayPal approval URL</returns>
        [HttpPost]
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto dto)
        {
            if (dto == null || dto.Amount <= 0)
                return BadRequest(new { message = "Invalid payment amount" });

            try
            {
                var (paymentId, approveUrl) = await _paymentService.CreateOrderAsync(dto.Amount, dto.OrderID);

                var payment = await _paymentRepository.GetByIdAsync(paymentId);

                if (payment == null)
                    return BadRequest(new { message = "Payment could not be created" });

                var response = MapPaymentToDto(payment, approveUrl);

                return Ok(new
                {
                    payment = response,
                    approveUrl,
                    message = "PayPal order created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,  $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get all payments (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPayments()
        {
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
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentById(string id)
        {
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
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments(int userId)
        {
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
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentDto dto)
        {
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
                        (string.IsNullOrEmpty(payment.PayPalOrderId) ? "" : PayPalSandboxApproveBase + payment.PayPalOrderId)
                }
            };
        }
    }
}
