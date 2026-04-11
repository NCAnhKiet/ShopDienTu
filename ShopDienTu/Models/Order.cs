using ShopDienTu.MoDels;

namespace ShopDienTu.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }

        
        public string? ReceiverName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        public Customer Customer { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
