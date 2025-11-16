using System.Security.Claims;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using DepiFinalProject.Repositories;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Http;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Models;

namespace PayPalAdvancedIntegration.Services
{
    public class PayPalService : IPaymentService
    {
        private readonly OrdersController _ordersController;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        protected readonly IOrderService _orderService;

        public PayPalService(
            Microsoft.Extensions.Configuration.IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IOrderService orderService) 
        {
            _httpContextAccessor = httpContextAccessor;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository; 
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            var clientId = config["PayPal:ClientId"];
            var secret = config["PayPal:ClientSecret"];

            var client = new PaypalServerSdkClient.Builder()
                .Environment(PaypalServerSdk.Standard.Environment.Sandbox)
                .ClientCredentialsAuth(new ClientCredentialsAuthModel.Builder(clientId, secret).Build())
                .Build();

            _ordersController = client.OrdersController;
        }

        public async Task<(string paymentid, string approveUrl)> CreateOrderAsync(decimal amount, int orderid)
        {
            // Get current user ID from JWT
            var userClaim = _httpContextAccessor.HttpContext?.User.FindFirst("userId");
            if (userClaim == null)
                throw new Exception("User ID claim not found");

            var userId = int.Parse(userClaim.Value);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"User with ID {userId} not found");

            var order = await _orderService.GetByIdAsync(orderid);
            if (order == null)
                throw new Exception($"Order with ID {orderid} not found");

            var createOrderInput = new CreateOrderInput
            {
                Body = new OrderRequest
                {
                    Intent = CheckoutPaymentIntent.Capture,
                    PurchaseUnits = new List<PurchaseUnitRequest>
                    {
                        new() { Amount = new AmountWithBreakdown { CurrencyCode = "USD", MValue = amount.ToString("0.00") } }
                    }
                }
            };

            var result = await _ordersController.CreateOrderAsync(createOrderInput);
            var orderId = result.Data.Id;
            var approveUrl = result.Data.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

            // Save Payment
            var payment = new Payment
            {
                UserId = userId,
                User = user,
                OrderID = orderid,
                Order = order,
                PayPalOrderId = orderId,
                Amount = amount,
                Status = "Pending",
                PaymentMethod = "PayPal",
                PaidAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);

            return (orderId, approveUrl);
        }

        public async Task<PaypalServerSdk.Standard.Models.Order> CaptureOrderAsync(string paymentid)
        {
            var result = await _ordersController.CaptureOrderAsync(new CaptureOrderInput { Id = paymentid });

            var status = result.Data.Status.HasValue ? result.Data.Status.Value.ToString() : "Unknown";
            var amount = result.Data.PurchaseUnits.First().Amount.MValue != null
                ? decimal.Parse(result.Data.PurchaseUnits.First().Amount.MValue)
                : 0;

            // Find existing payment record
            var payments = await _paymentRepository.GetAllAsync();
            var payment = payments.FirstOrDefault(p => p.PayPalOrderId == paymentid);

            if (payment != null)
            {
                payment.Status = status;
                payment.Amount = amount;
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
            }

            return result.Data;
        }
    }
}
