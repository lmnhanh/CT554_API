namespace CT554_API.Models
{
    public class CartDTO
    {
        public string? Id { get; set; }
        public float Quantity { set; get; } = 1;
        public float RealQuantity { set; get; } = 1;
        public int ProductDetailId { set; get; }
        public string? UserId { set; get; }

    }
}
