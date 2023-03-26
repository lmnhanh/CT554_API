
namespace CT554_API.Models
{
    public class InvoiceDetailInfo
    {
        public int ProductDetailId { get; set; }
        public ProductDetailInfo? ProductDetail { get; set; }
        public float Quantity { get; set; } = 1.0f;
    }
}
