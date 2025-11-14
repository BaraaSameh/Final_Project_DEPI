using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentService _paymentService;

        // NOTE: consider injecting IConfiguration to build URL from config instead of hardcoding.
        private const string PayPalSandboxApproveBase = "https://www.sandbox.paypal.com/checkoutnow?token=";

        public PaymentsController(IPaymentRepository paymentRepository, IPaymentService paymentService)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize(Roles = "admin,client")]

        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto dto)
        {
            try {
                if (dto == null || dto.Amount <= 0)
                    return BadRequest(new { message = "Invalid payment amount" });

                // Create the PayPal order (service returns PayPal order id and approval url)
                var (paymentid, approveUrl) = await _paymentService.CreateOrderAsync(dto.Amount, dto.OrderID);

                var payment = (await _paymentRepository.GetAllAsync())
                                 .FirstOrDefault(p => p.PayPalOrderId == paymentid);

                if (payment == null)
                    return BadRequest(new { message = "Payment could not be created" });

                var response = new PaymentResponseDto
                {
                    PaymentID = payment.PaymentID,
                    OrderID = payment.OrderID ?? 0,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    PaymentMethod = payment.PaymentMethod,
                    PaidAt = payment.PaidAt,
                    Order = payment.Order != null
                        ? new OrderSummaryDto
                        {
                            OrderNumber = payment.Order.OrderNo,
                            TotalAmount = payment.Order.TotalAmount
                        }
                        : null,
                    User = payment.User != null
                        ? new UserSummaryDto
                        {
                            UserId = payment.User.UserID,
                            Email = payment.User.UserEmail
                        }
                        : null,
                    PaymentDetails = new PaymentDetailsDto
                    {
                        PayPalOrderId = payment.PayPalOrderId,
                        PayPalLink = approveUrl ?? string.Empty
                    }
                };

                return Ok(new
                {
                    payment = response,
                    approveUrl,
                    message = "PayPal order created successfully"
                });
            }
            catch(Exception ex) {
                return StatusCode(500, new { message = "An error occurred while retrieving payments", error = ex.Message });
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            try {
                var payments = await _paymentRepository.GetAllAsync();

                var paymentDtos = payments.Select(p => new PaymentResponseDto
                {
                    PaymentID = p.PaymentID,
                    OrderID = p.OrderID ?? 0,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    PaidAt = p.PaidAt,
                    Order = p.Order == null ? null : new OrderSummaryDto
                    {
                        OrderNumber = p.Order.OrderNo,
                        TotalAmount = p.Order.TotalAmount
                    },
                    User = p.User == null ? null : new UserSummaryDto
                    {
                        UserId = p.User.UserID,
                        Email = p.User.UserEmail
                    },
                    PaymentDetails = new PaymentDetailsDto
                    {
                        PayPalOrderId = p.PayPalOrderId,
                        PayPalLink = string.IsNullOrEmpty(p.PayPalOrderId) ? string.Empty : PayPalSandboxApproveBase + p.PayPalOrderId
                    }
                }).ToList();

                return Ok(paymentDtos);
            }
           
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving payments", error = ex.Message });
            }
            
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> GetPaymentById(string id)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                    return NotFound(new { message = "Payment not found" });

                var userId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("Admin");
                if (payment.UserId != userId && !isAdmin)
                    return Forbid();

                var dto = new PaymentResponseDto
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
                        UserId = payment.UserId,
                        Email = payment.User.UserEmail
                    },
                    PaymentDetails = new PaymentDetailsDto
                    {
                        PayPalOrderId = payment.PayPalOrderId,
                        PayPalLink = string.IsNullOrEmpty(payment.PayPalOrderId) ? string.Empty : PayPalSandboxApproveBase + payment.PayPalOrderId
                    }
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the payment", error = ex.Message });
            }
        }
           

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> GetUserPayments(int userId)
        {
            try {
                var currentUserId = int.Parse(User.FindFirst("userId")!.Value);
                var isAdmin = User.IsInRole("Admin");

                if (currentUserId != userId && !isAdmin)
                    return Forbid();

                var payments = await _paymentRepository.GetByUserIdAsync(userId);

                var paymentDtos = payments.Select(p => new PaymentResponseDto
                {
                    PaymentID = p.PaymentID,
                    OrderID = p.OrderID ?? 0,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    PaidAt = p.PaidAt,
                    Order = p.Order == null ? null : new OrderSummaryDto
                    {
                        OrderNumber = p.Order.OrderNo,
                        TotalAmount = p.Order.TotalAmount
                    },
                    User = p.User == null ? null : new UserSummaryDto
                    {
                        UserId = p.User.UserID,
                        Email = p.User.UserEmail
                    },
                    PaymentDetails = new PaymentDetailsDto
                    {
                        PayPalOrderId = p.PayPalOrderId,
                        PayPalLink = string.IsNullOrEmpty(p.PayPalOrderId) ? string.Empty : PayPalSandboxApproveBase + p.PayPalOrderId
                    }
                }).ToList();

                return Ok(paymentDtos);
            }
            
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user payments", error = ex.Message });
            }
            
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentDto dto)
        {
            try {
                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                    return NotFound(new { message = "Payment not found" });

                payment.Status = dto.Status ?? payment.Status;
                payment.PaymentMethod = dto.PaymentMethod ?? payment.PaymentMethod;

                await _paymentRepository.UpdateAsync(payment);

                // reload with navigation properties (repository already includes Order & User)
                var updated = await _paymentRepository.GetByIdAsync(id);

                var response = new PaymentResponseDto
                {
                    PaymentID = updated.PaymentID,
                    OrderID = updated.OrderID ?? 0,
                    Amount = updated.Amount,
                    Status = updated.Status,
                    PaymentMethod = updated.PaymentMethod,
                    PaidAt = updated.PaidAt,
                    Order = updated.Order == null ? null : new OrderSummaryDto
                    {
                        OrderNumber = updated.Order.OrderNo,
                        TotalAmount = updated.Order.TotalAmount
                    },
                    User = updated.User == null ? null : new UserSummaryDto
                    {
                        UserId = updated.User.UserID,
                        Email = updated.User.UserEmail
                    },
                    PaymentDetails = new PaymentDetailsDto
                    {
                        PayPalOrderId = updated.PayPalOrderId,
                        PayPalLink = string.IsNullOrEmpty(updated.PayPalOrderId) ? string.Empty : PayPalSandboxApproveBase + updated.PayPalOrderId
                    }
                };

                return Ok(new { message = "Payment updated successfully", payment = response });
            } catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IActionResult>.ErrorResponse(
                    "An error occurred during payment update", new List<string> { ex.Message }));
            }
            
           
        }
    }
}