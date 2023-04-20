namespace CT554_API.Models
{
    public class CartInfo
    {
        public string Id { get; set; } = string.Empty;
        public float Quantity { set; get; } = 1;
        public float RealQuantity { set; get; } = 0;
        public bool IsAvailable { get; set; } = true;
        public string? OrderId { set; get; }
        public string UserId { get; set; } = string.Empty;
        public virtual ProductDetailInfo ProductDetail { get; set; } = null!;
    }
}
