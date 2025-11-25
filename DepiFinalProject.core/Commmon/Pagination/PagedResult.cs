using CloudinaryDotNet.Actions;

namespace DepiFinalProject.Core.Commmon.Pagination
{
  
        /// <summary>
        /// Generic wrapper for paginated results with metadata
        /// </summary>
        public class PagedResult<T>
        {
            public List<T> Data { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalRecords { get; set; }
            public int TotalPages { get; set; }
            public bool HasPreviousPage { get; set; }
            public bool HasNextPage { get; set; }

            public PagedResult(List<T> data, int pageNumber, int pageSize, int totalRecords)
            {
                Data = data;
                PageNumber = pageNumber;
                PageSize = pageSize;
                TotalRecords = totalRecords;
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                HasPreviousPage = pageNumber > 1;
                HasNextPage = pageNumber < TotalPages;
            }
        }
    }

    
