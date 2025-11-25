using CloudinaryDotNet.Actions;

namespace DepiFinalProject.Core.Commmon.Pagination
{
    public class PaginationParameters
    {
      
        /// <summary>
        /// Base class for pagination parameters with validation
        /// </summary>
       
            private const int MaxPageSize = 100;
            private const int MinPageSize = 1;
            private const int DefaultPageSize = 10;

            private int _pageNumber = 1;
            private int _pageSize = DefaultPageSize;

            public int PageNumber
            {
                get => _pageNumber;
                set => _pageNumber = value < 1 ? 1 : value;
            }

            public int PageSize
            {
                get => _pageSize;
                set
                {
                    if (value < MinPageSize)
                        _pageSize = MinPageSize;
                    else if (value > MaxPageSize)
                        _pageSize = MaxPageSize;
                    else
                        _pageSize = value;
                }
            }
        }
    }
