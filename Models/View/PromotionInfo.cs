namespace CT554_API.Models.View
{
    public class PromotionInfo
    {
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { set; get; } = null!;
        public DateTime DateStart { set; get; }
        public DateTime DateEnd { set; get; }
        public DateTime DateCreate { set; get; }
        public int Stock { get; set; } = 1;
        public bool IsPercentage { get; set; } = true;
        public long Discount { set; get; } = 5L;
        public bool IsActive { set; get; } = true;
        public virtual ICollection<ProductInfo>? Products { get; set; }
    }
}
