namespace CT554_API.Models
{
    public class ProductDetailInfo
    {
        public int Id { get; set; }
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ToWholesale { get; set; } = 1;
        public bool IsAvailable { get; set; } = true;
        public long ImportPrice { get; set; }
        public long RetailPrice { get; set; }
        public long WholePrice { get; set; }
        public float Stock { get; set; }

        public string ProductName { get; set; } = "";
    }
}
