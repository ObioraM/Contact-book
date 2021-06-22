using System;
using System.Collections.Generic;
using System.Text;

namespace ContactAPI.DTO
{
    public class PagingDto
    {
        const int MaxPageSize = 20;
        public int PageNumber { get; set; } = 1;

        private int _PageSize { get; set; } = 10;

        public int PageSize
        {
            get { return _PageSize; }
            set { _PageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
        public string QuerySearch { get; set; }
    }
}
