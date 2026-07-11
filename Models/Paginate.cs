namespace ShoppingCart.Models
{
    public class Paginate
    {
        public int TolalItems { get; set; } // Tổng số Items
        public int PageSize { get; set; } // Số lượng Items trên 1 trang
        public int CurrentPage { get; set; } // Trang hiện tại
        public int TotalPages { get; set; } // Tổng số trang
        public int StartPage { get; set; } // Trang bắt đầu
        public int EndPage { get; set; } // Trang kết thúc

        public Paginate()
        {

        }

        // Tính toán phân trang
        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            // Làm tròn tổng số Items trên 1 trang Vd: 16 Items / 10  => làm tròn = 3 trang
            int totalPage = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);

            // page hiện tại = 1
            int currentPage = page;

            int startPage = currentPage - 5; // Trang bắt đầu = trang hiện tại - 5
            int endPage = currentPage + 4; // Trang kết thúc = trang hiện tại + 4

            if (startPage <= 0)
            {
                endPage = endPage-(startPage-1);
                startPage = 1;
            }

            if (endPage > totalPage)
            {
                endPage = totalPage;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TolalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPage;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
