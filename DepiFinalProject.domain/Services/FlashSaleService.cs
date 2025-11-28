using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Services
{
    public class FlashSaleService : IFlashSaleService
    {
        private readonly IFlashSaleRepository _repository;

        public FlashSaleService(IFlashSaleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FlashSaleDto>> GetAllAsync()
        {
            var flashSales = await _repository.GetAllAsync();

            if (flashSales == null)
                throw new Exception("Failed to retrieve flash sale.");

            return flashSales.Select(fs => new FlashSaleDto
            {
                FlashSaleID = fs.FlashSaleID,
                Title = fs.Title,
                StartDate = fs.StartDate,
                EndDate = fs.EndDate,
                IsActive = fs.IsActive,
                MaxUsers = fs.MaxUsers,
                CreatedAt = fs.CreatedAt,
                ProductCount = fs.FlashSaleProducts?.Count ?? 0
            });
        }

        public async Task<FlashSaleDto?> GetByIdAsync(int id)
        {
            var flashSale = await _repository.GetByIdAsync(id);

            if (flashSale == null)
                throw new Exception("Failed to retrieve flash sale.");

            return new FlashSaleDto
            {
                FlashSaleID = flashSale.FlashSaleID,
                Title = flashSale.Title,
                StartDate = flashSale.StartDate,
                EndDate = flashSale.EndDate,
                IsActive = flashSale.IsActive,
                MaxUsers = flashSale.MaxUsers,
                CreatedAt = flashSale.CreatedAt,
                ProductCount = flashSale.FlashSaleProducts?.Count ?? 0
            };
        }

        public async Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto)
        {
            var flashSale = new FlashSale
            {
                Title = dto.Title,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MaxUsers = dto.MaxUsers
            };

            var created = await _repository.CreateAsync(flashSale);

            return new FlashSaleDto
            {
                FlashSaleID = created.FlashSaleID,
                Title = created.Title,
                StartDate = created.StartDate,
                EndDate = created.EndDate,
                IsActive = created.IsActive,
                MaxUsers = created.MaxUsers,
                CreatedAt = created.CreatedAt,
                ProductCount = 0
            };
        }

        public async Task<FlashSaleDto?> UpdateAsync(int id, UpdateFlashSaleDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Title))
                existing.Title = dto.Title;

            if (dto.StartDate.HasValue)
                existing.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                existing.EndDate = dto.EndDate.Value;

            if (dto.IsActive.HasValue)
                existing.IsActive = dto.IsActive.Value;

            if (dto.MaxUsers.HasValue)
                existing.MaxUsers = dto.MaxUsers;

            var updated = await _repository.UpdateAsync(existing);

            if (updated == null)
                return null;

            return new FlashSaleDto
            {
                FlashSaleID = updated.FlashSaleID,
                Title = updated.Title,
                StartDate = updated.StartDate,
                EndDate = updated.EndDate,
                IsActive = updated.IsActive,
                MaxUsers = updated.MaxUsers,
                CreatedAt = updated.CreatedAt,
                ProductCount = updated.FlashSaleProducts?.Count ?? 0
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}