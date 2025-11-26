using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Models;


public class ReturnService : IReturnService
{
    private readonly IReturnRepository _returnRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProductRepository _productRepository;
    private readonly PaymentsController _paymentsController;

    public ReturnService(
        Microsoft.Extensions.Configuration.IConfiguration config,
        IReturnRepository returnRepository,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IProductRepository productRepository)
    {
        _returnRepository = returnRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _productRepository = productRepository;
        var clientId = config["PayPal:ClientId"];
        var secret = config["PayPal:ClientSecret"];

        var client = new PaypalServerSdkClient.Builder()
            .Environment(PaypalServerSdk.Standard.Environment.Sandbox)
            .ClientCredentialsAuth(new ClientCredentialsAuthModel.Builder(clientId, secret).Build())
            .Build();
       _paymentsController=client.PaymentsController;
    }

    public async Task<IEnumerable<Return>> GetAllReturnsAsync()
    {
        return await _returnRepository.GetAllAsync();
    }

    public async Task<Return?> GetReturnByIdAsync(int id)
    {
        return await _returnRepository.GetByIdAsync(id);
    }

    public async Task<Return?> GetReturnsByOrderItemIdAsync(int orderItemId)
    {
        return await _returnRepository.GetByOrderItemIdAsync(orderItemId);
    }
    public async Task<IEnumerable<Return?>> GetReturnRequestsByUserIdAsync(int userId)
    {
        return await _returnRepository.GetByUserIdAsync(userId);
    }

    public async Task<Return> RequestReturnAsync(int userId, int orderItemId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        // Get order item to validate
        var orderItem = await _orderRepository.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            throw new InvalidOperationException("Order item not found.");

        // Get the order to check payment status
        var order = await _orderRepository.GetByIdAsync(orderItem.OrderID);
        if (order == null)
            throw new InvalidOperationException("Order not found.");

        // Only allow returns for delivered orders
        if (order.OrderStatus != "Delivered" && order.OrderStatus != "Paid")
            throw new InvalidOperationException("Can only return items from delivered or paid orders.");

        // Check if return window is still valid (e.g., 30 days)
        if (order.OrderDate.AddDays(30) < DateTime.UtcNow)
            throw new InvalidOperationException("Return window has expired (30 days from order date).");

        // Prevent duplicate return requests
        var existing = await _returnRepository.GetByOrderItemIdAsync(orderItemId);
        if (existing != null)
            throw new InvalidOperationException("A return request for this order item already exists.");

        var newReturn = new Return
        {
            UserId = userId,
            OrderItemID = orderItemId,
            Reason = reason,
            Status = "pending", 
            RequestedAt = DateTime.UtcNow,
            RefundStatus = "pending"
        };

        return await _returnRepository.CreateAsync(newReturn);
    }

    public async Task<bool> UpdateReturnStatusAsync(int id, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentNullException(nameof(status));

        var returnRequest = await _returnRepository.GetByIdAsync(id);
        if (returnRequest == null)
            return false;

        // If approving a return, trigger refund process
        if (status.ToLower() == "approved" && returnRequest.RefundStatus != "completed")
        {
            try
            {
                await ProcessRefundAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process refund: {ex.Message}");
            }
        }

        return await _returnRepository.UpdateStatusAsync(id, status);
    }

    public async Task<Return> ProcessRefundAsync(int returnId)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId);
        if (returnRequest == null)
            throw new InvalidOperationException("Return request not found.");

        if (returnRequest.Status.ToLower() != "approved")
            throw new InvalidOperationException("Return must be approved before processing refund.");

        if (returnRequest.RefundStatus == "completed")
            throw new InvalidOperationException("Refund already processed for this return.");

        // Get order item
        var orderItem = await _orderRepository.GetOrderItemByIdAsync(returnRequest.OrderItemID);
        if (orderItem == null)
            throw new InvalidOperationException("Order item not found.");

        // Get order
        var order = await _orderRepository.GetByIdAsync(orderItem.OrderID);
        if (order == null)
            throw new InvalidOperationException("Order not found.");

        // Find payment for this order
        var payments = await _paymentRepository.GetAllAsync();
        var payment = payments.FirstOrDefault(p => p.OrderID == order.OrderID && p.Status == "COMPLETED");

        if (payment == null)
            throw new InvalidOperationException("No completed payment found for this order.");

        // Calculate refund amount (price * quantity for this item)
        decimal refundAmount = orderItem.Price * orderItem.Quantity;

        try
        {
            // CRITICAL: Use the CaptureID, not the OrderID for refunds
            var captureId = payment.PayPalCaptureId;

            if (string.IsNullOrEmpty(captureId))
                throw new InvalidOperationException("Payment capture ID not found. Cannot process refund.");

            var refundRequest = new RefundRequest
            {
                Amount = new Money
                {
                    CurrencyCode = "EGP",
                    MValue = refundAmount.ToString("0.00")
                },
                NoteToPayer = $"Refund for return request #{returnId}. Reason: {returnRequest.Reason}"
            };

            var refundResult = await _paymentsController.RefundCapturedPaymentAsync(
                new RefundCapturedPaymentInput
                {
                    CaptureId = captureId,
                    Body = refundRequest
                }
            );
            
            // Update return record with refund information
            returnRequest.RefundId = refundResult.Data.Id;
            returnRequest.RefundAmount = refundAmount;
            returnRequest.RefundedAt = DateTime.UtcNow;
            returnRequest.RefundStatus = refundResult.Data.Status?.ToString().ToLower() ?? "pending";
            returnRequest.Status = "approved";

            await _returnRepository.UpdateStatusAsync(returnId, "approved");

            // Restore product stock
            var product = await _productRepository.GetByIdAsync(orderItem.ProductID);
            if (product != null)
            {
                product.Stock += orderItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            return returnRequest;
        }
        catch (Exception ex)
        {
            returnRequest.RefundStatus = "failed";
            await _returnRepository.UpdateStatusAsync(returnId, "failed");
            throw new InvalidOperationException($"PayPal refund failed: {ex.Message}");
        }
    }


    public async Task<bool> DeleteReturnAsync(int id)
    {
        return await _returnRepository.DeleteReturnAsync(id);
    }

    public async Task<bool> CancelReturnAsync(int id)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(id);
        if (returnRequest == null)
            return false;

        // Don't allow cancellation if refund is already processed
        if (returnRequest.RefundStatus == "completed")
            throw new InvalidOperationException("Cannot cancel return - refund already processed.");

        return await _returnRepository.CancelAsync(id);
    }
}
