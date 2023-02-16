using System.ComponentModel.DataAnnotations.Schema;

namespace CT554_API.Entity
{
	public class Cart
	{
		public Guid Id { get; set; }
		public int Quantity { set; get; } = 1;
		public bool IsAvailable { get; set; } = true;
		public int ProductDetailId { set; get; }
		public Guid? OrderId { set; get; }
		public string UserId { set; get; } = null!;

		public virtual User? User { get; set; }
		public virtual ProductDetail? ProductDetail { get; set; }
		public virtual Order? Order { get; set; }
	}
}
