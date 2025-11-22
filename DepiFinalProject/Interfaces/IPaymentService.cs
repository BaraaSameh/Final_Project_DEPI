using System.Threading.Tasks;
using PaypalServerSdk.Standard.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a PayPal order and returns the order ID and approval URL.
        /// </summary>
        /// <param name="amount">Amount to charge in USD.</param>
        /// <returns>A tuple of (orderId, approveUrl).</returns>
        Task<(string paymentid, string approveUrl)> CreateOrderAsync(decimal amount, int orderid);

        /// <summary>
        /// Captures a PayPal order and updates payment status.
        /// </summary>
        /// <param name="paymentid">The PayPal order ID.</param>
        /// <returns>The captured PayPal order details.</returns>
        Task<Order> CaptureOrderAsync(string paymentid);

    }
}
