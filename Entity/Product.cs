namespace CT554_API.Entity
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string WellKnownId { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;
		public bool IsRecommended { get; set; } = false;

		public int CategoryId { get; set; }
		public virtual Category? Category { get; set; }

		public virtual ICollection<ProductDetail>? Details { get; set; }
		public virtual ICollection<Image>? Images { get; set; }	
	}
}
