namespace CT554_API.Models
{
	public class ProductDTO
	{
		public int? Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string WellKnownId { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;
		public bool IsRecommended { get; set; } = false;
		public int? CategoryId { get; set; }
	}
}
