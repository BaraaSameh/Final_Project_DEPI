using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Core.Commmon.Pagination
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Extension methods for IQueryable to enable easy pagination
        /// </summary>
        
            /// <summary>
            /// Applies pagination to IQueryable and returns PagedResult
            /// </summary>
            public static async Task<PagedResult<T>> ToPaginatedListAsync<T>(
                this IQueryable<T> source,
                int pageNumber,
                int pageSize)
            {
                // Get total count before pagination
                var totalRecords = await source.CountAsync();

                // Apply pagination
                var items = await source
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<T>(items, pageNumber, pageSize, totalRecords);
            }
        }
    }

