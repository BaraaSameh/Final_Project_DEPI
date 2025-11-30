using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Http;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Models;
using static DepiFinalProject.Core.DTOs.OrderDto;
namespace PayPalAdvancedIntegration.Services
{
    public class PayPalService : IPaymentService
    {
        private readonly OrdersController _ordersController;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderService _orderService;
        private readonly IOtpService _otpService;


        public PayPalService(
            Microsoft.Extensions.Configuration.IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IOrderService orderService,
            IOtpService otpService)
        {
            _httpContextAccessor = httpContextAccessor;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _orderService = orderService;

            var client = new PaypalServerSdkClient.Builder()
                .Environment(PaypalServerSdk.Standard.Environment.Sandbox)
                .ClientCredentialsAuth(
                    new ClientCredentialsAuthModel.Builder(
                        config["PayPal:ClientId"],
                        config["PayPal:ClientSecret"]
                    ).Build()
                )
                .Build();

            _ordersController = client.OrdersController;
            _otpService = otpService;
        }


        public async Task<(string paymentid, string approveUrl)> CreateOrderAsync(decimal amount, int orderid)
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("userId")!.Value);
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");
            var order = await _orderService.GetByIdAsync(orderid) ?? throw new Exception("Order not found");

            var createRequestId = "ORDER-" + Guid.NewGuid().ToString("N").ToUpper();

            var createOrderInput = new CreateOrderInput
            {
                Body = new OrderRequest
                {
                    Intent = CheckoutPaymentIntent.Capture,
                    PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    Amount = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        MValue = amount.ToString("0.00")
                    },
                    ReferenceId = orderid.ToString()
                }
            },
                    ApplicationContext = new OrderApplicationContext
                    {
                        BrandName = "Zenon Market",
                        ReturnUrl = "https://zenon-ecomm-mega-project.vercel.app/MyOrder",
                        CancelUrl = "https://zenon-ecomm-mega-project.vercel.app/checkout"
                    }
                },
                PaypalRequestId = createRequestId
            };

            var result = await _ordersController.CreateOrderAsync(createOrderInput);


            var paypalOrderId = result.Data.Id;
            var approveUrl = result.Data.Links.First(l => l.Rel == "approve").Href;

            // Save local payment
            var payment = new Payment
            {
                UserId = userId,
                User = user,
                OrderID = orderid,
                Order = order,
                PayPalOrderId = paypalOrderId,
                Amount = amount,
                Status = "Pending",
                PaymentMethod = "PayPal",
                PaidAt = DateTime.Now,
                PayPalCaptureId = null
            };

            await _paymentRepository.CreateAsync(payment);

            return (paypalOrderId, approveUrl);
        }


        public async Task<PaypalServerSdk.Standard.Models.Order> CaptureOrderAsync(string paypalOrderId)
        {
            if (string.IsNullOrWhiteSpace(paypalOrderId))
                throw new ArgumentException("PayPal Order ID is required.", nameof(paypalOrderId));


            var paypalRequestId = "CAPTURE-" + Guid.NewGuid().ToString("N").ToUpper();

            try
            {
                var result = await _ordersController.CaptureOrderAsync(
                    new CaptureOrderInput
                    {
                        Id = paypalOrderId,
                        Body = new OrderCaptureRequest(),
                        PaypalRequestId = paypalRequestId
                    });

                var capturedOrder = result.Data;
                var status = capturedOrder.Status?.ToString() ?? "UNKNOWN";
                var purchaseUnit = capturedOrder.PurchaseUnits?.FirstOrDefault();
                var capture = purchaseUnit?.Payments?.Captures?.FirstOrDefault();

                string? captureId = capture?.Id;
                decimal amount = 0;
                if (purchaseUnit?.Amount?.MValue != null)
                    decimal.TryParse(purchaseUnit.Amount.MValue, out amount);

                var payment = await _paymentRepository.GetByPayPalOrderIdAsync(paypalOrderId);
                if (payment != null)
                {
                    payment.Status = status;
                    payment.PayPalCaptureId = captureId;

                    if (string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                    {
                        payment.PaidAt = DateTime.UtcNow;
                        await _otpService.SendPaymentInvoiceAsync(payment);
                    }


                    await _paymentRepository.UpdateAsync(payment);

                    if (status.ToLower() == "completed" && payment.OrderID.HasValue)
                    {
                        await _orderService.UpdateStatusAsync(payment.OrderID.Value, new UpdateOrderStatusDTO
                        {
                            OrderStatus = "Paid"
                        });
                    }
                }

                return capturedOrder;
            }
            catch (PaypalServerSdk.Standard.Exceptions.ApiException apiEx)
            {
                throw new Exception($"PayPal Capture Failed (Request-Id: {paypalRequestId})", apiEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error during PayPal capture (Request-Id: {paypalRequestId})", ex);
            }
        }
        public async Task<Payment?> CancelPayment(string paypalOrderId)
        {
            var payment = await _paymentRepository.GetByPayPalOrderIdAsync(paypalOrderId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found for the specified order.");
            payment.Status = "Cancelled";
            await _orderService.CancelAsync(payment.OrderID.Value);
         
            await _paymentRepository.UpdateAsync(payment);
            return payment;


        }
    }
}

