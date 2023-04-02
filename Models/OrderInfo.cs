namespace CT554_API.Models
{
    public class OrderInfo
    {
        public Guid Id { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;
        public DateTime? DateSuccess { get; set; }
        public DateTime? DateProccesed { get; set; }
        public string Description { get; set; } = "";
        public bool IsProccesed { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
        public long Total { get; set; }
        public UserInfo User { get; set; } = null!;
        public IEnumerable<CartInfo> Carts { get; set; } = null!;
    }
}
