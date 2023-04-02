namespace CT554_API.Models
{
    public class OrderDTO
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public float Total { get; set; } = 0;
        public List<CartDTO> Carts { get; set; } = new List<CartDTO>();
    }
}
