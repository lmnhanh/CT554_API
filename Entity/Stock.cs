namespace CT554_API.Entity
{
	public class Stock
	{
		public DateTime DateUpdate { get; set; } = DateTime.UtcNow.Date;
		public bool IsManualUpdate { get; set; } = false;
		public float Value { get; set; } = 1.0f;
		public int ProductDetailId { get; set; }
		public virtual ProductDetail? ProductDetail { get; set; }
		
	}
}
