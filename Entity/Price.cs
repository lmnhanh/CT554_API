namespace CT554_API.Entity
{
	public class Price
	{
		public int Id { get; set; }
		public long Value { get; set; } = 0L;
		public bool IsRetailPrice { get; set; } = true;
		public bool IsImportPrice { get; set; } = false;
		public DateTime DateApply { get; set; } = DateTime.UtcNow;

		public int ProductDetailId { get; set; }
		public virtual ProductDetail? ProductDetail { get; set; }
	}
}
