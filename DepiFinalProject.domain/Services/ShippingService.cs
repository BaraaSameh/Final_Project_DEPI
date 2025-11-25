using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Services
{
    public class ShippingService : IShippingService
    {

        private readonly IShippingRepository _shippingRepository;

        public ShippingService(IShippingRepository shippingRepository)
        {
            _shippingRepository = shippingRepository;
        }

        public async Task<ICollection<Shipping>> GetAllShippingsAsync()
        {
            try
            {
                return await _shippingRepository.GetAllShippingsAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving shippings", ex);
            }
        }

        public async Task<Shipping?> GetShippingByIdAsync(int shippingId)
        {
            try
            {
                if (shippingId <= 0)
                    throw new ArgumentException("Shipping ID must be greater than zero", nameof(shippingId));

                return await _shippingRepository.GetShippingByIdAsync(shippingId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while retrieving shipping with ID {shippingId}", ex);
            }
        }

        public async Task<Shipping> CreateShippingAsync(Shipping newShipping)
        {
            try
            {
                if (newShipping == null)
                    throw new ArgumentNullException(nameof(newShipping), "Shipping cannot be null");

                ValidateShipping(newShipping);

                return await _shippingRepository.CreateShippingAsync(newShipping);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while creating shipping", ex);
            }
        }

        public async Task<Shipping?> UpdateShippingAsync(Shipping updatedShipping)
        {
            try
            {
                if (updatedShipping == null)
                    throw new ArgumentNullException(nameof(updatedShipping), "Shipping cannot be null");

                if (updatedShipping.ShippingID <= 0)
                    throw new ArgumentException("Shipping ID must be greater than zero", nameof(updatedShipping));

                ValidateShipping(updatedShipping);

                var result = await _shippingRepository.UpdateShippingAsync(updatedShipping);

                return result;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while updating shipping with ID {updatedShipping.ShippingID}", ex);
            }
        }

        public async void UpdateShippingStatus(int shippingId, string newStatus)
        {
            try
            {
                if (shippingId <= 0)
                    throw new ArgumentException("Shipping ID must be greater than zero", nameof(shippingId));

                if (string.IsNullOrWhiteSpace(newStatus))
                    throw new ArgumentException("Status cannot be empty", nameof(newStatus));

                var shipping = await _shippingRepository.GetShippingByIdAsync(shippingId);

                if (shipping == null)
                    throw new KeyNotFoundException($"Shipping with ID {shippingId} not found");

                shipping.ShippingStatus = newStatus;
                await _shippingRepository.UpdateShippingAsync(shipping);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async void CalcEstimatedDelivery(int shippingId, DateTime newEstimatedDate)
        {
            try
            {
                if (shippingId <= 0)
                    throw new ArgumentException("Shipping ID must be greater than zero", nameof(shippingId));

                if (newEstimatedDate < DateTime.UtcNow)
                    throw new ArgumentException("Estimated delivery date cannot be in the past", nameof(newEstimatedDate));

                var shipping = await _shippingRepository.GetShippingByIdAsync(shippingId);

                if (shipping == null)
                    throw new KeyNotFoundException($"Shipping with ID {shippingId} not found");

                shipping.EstimatedDelivery = newEstimatedDate;
                await _shippingRepository.UpdateShippingAsync(shipping);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ValidateShipping(Shipping shipping)
        {
            if (string.IsNullOrWhiteSpace(shipping.CompanyName))
                throw new ArgumentException("Company name is required", nameof(shipping.CompanyName));

            if (string.IsNullOrWhiteSpace(shipping.TrackingNumber))
                throw new ArgumentException("Tracking number is required", nameof(shipping.TrackingNumber));

            if (string.IsNullOrWhiteSpace(shipping.ShippingStatus))
                throw new ArgumentException("Shipping status is required", nameof(shipping.ShippingStatus));

            if (shipping.EstimatedDelivery < DateTime.UtcNow)
                throw new ArgumentException("Estimated delivery date cannot be in the past", nameof(shipping.EstimatedDelivery));
        }
    }
}
