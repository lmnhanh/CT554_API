using System.ComponentModel.DataAnnotations;

namespace CT554_API.Entity
{
	public class Promotion
	{
		public string Id { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Description { set; get; } = null!;
		public DateTime ApplyFrom { set; get; } = DateTime.Now;
		public DateTime DateEnd { set; get; } = DateTime.Now.AddDays(1);
		public int Stock { get; set; } = 1;
		public int DiscountPercent { set; get; } = 5;
		public long MaxDiscount { set; get; } = 1L;
		public bool IsActive { set; get; } = true;
		public int ProductId { set; get; }
		public virtual Product? Product { get; set; }
	}
}
