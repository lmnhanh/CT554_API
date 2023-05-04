namespace CT554_API.Models.View
{
    public class OrderInfo
    {
        public Guid Id { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;
        public DateTime? DateSuccess { get; set; }
        public DateTime? DateProcessed { get; set; }
        public string Description { get; set; } = "";
        public bool IsProcessed { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
        public long Total { get; set; }
        public UserInfo User { get; set; } = null!;
        public IEnumerable<CartInfo> Carts { get; set; } = null!;
    }
}
