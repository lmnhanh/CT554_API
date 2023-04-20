namespace CT554_API.Models
{
    public class ProductDetailModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public long ImportPrice { get; set; } = 1L;
        public long RetailPrice { get; set; } = 1L;
        public long WholePrice { get; set; } = 1L;
        public int ToWholesale { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;
    }
}
