using System.Security.Claims;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.Core.DTOs.OrderDto;

namespace DepiFinalProject.Controllers
{
    [ApiController]
     [Authorize]
    [Route("api/webhooks/paypal")]
    public class PayPalWebhookController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderService _orderService;

        public PayPalWebhookController(
            IPaymentRepository paymentRepository,
            IOrderService orderService)
        {
            _paymentRepository = paymentRepository;
            _orderService = orderService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // TODO: Validate signature here

            var json = Newtonsoft.Json.Linq.JObject.Parse(body);
            var eventType = json["event_type"]?.ToString();
            var resource = json["resource"];

            if (eventType == "CHECKOUT.ORDER.APPROVED")
            {
                string paypalOrderId = resource["id"].ToString();
                await HandlePaymentCompleted(paypalOrderId);
            }

            if (eventType == "PAYMENT.CAPTURE.COMPLETED")
            {
                string captureId = resource["id"].ToString();
                string paypalOrderId = resource["supplementary_data"]?["related_ids"]?["order_id"]?.ToString();

                await HandlePaymentCompleted(paypalOrderId);
            }

            return Ok();
        }

        private async Task HandlePaymentCompleted(string paypalOrderId)
        {
            var payment = await _paymentRepository.GetByPayPalOrderIdAsync(paypalOrderId);
            if (payment == null)
                return;

            payment.Status = "COMPLETED";
            payment.PaidAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            if (payment.OrderID.HasValue)
            {
                await _orderService.UpdateStatusAsync(payment.OrderID.Value, new UpdateOrderStatusDTO
                {
                    OrderStatus = "Paid"
                });
            }
        }
    }
}
