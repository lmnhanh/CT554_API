using CT554_Entity.Entity;

namespace CT554_API.Models
{
	public class PromotionDTO
	{
		public string? Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { set; get; } = null!;
		public DateTime DateStart { set; get; } = DateTime.UtcNow;
		public DateTime DateEnd { set; get; } = DateTime.UtcNow.AddDays(1);
		public int Stock { get; set; } = 1;
		public bool IsPercentage { get; set; } = true;
		public long Discount { set; get; } = 5L;
		public long MaxDiscount { set; get; } = 1L;
		public bool IsActive { set; get; } = true;
		public virtual ICollection<int>? ProductIds { get; set; }
	}
}
