using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class PaginatedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public PaginatedResultDto() { }

        public PaginatedResultDto(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            PageIndex = pageIndex;
            TotalPages = (int)System.Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
        }
    }
}
