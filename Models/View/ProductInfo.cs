namespace CT554_API.Models.View
{
    public class ProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string WellKnownId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DateUpdate { get; set; }
        public bool IsActive { get; set; }
        public bool IsRecommended { get; set; }
        public CategoryInfo Category { get; set; } = null!;
        public long CurrentDiscount { get; set; } = 0L;

        public IEnumerable<ProductDetailInfo>? Details { get; set; }
        public virtual ICollection<ImageInfo>? Images { get; set; }
    }
}
