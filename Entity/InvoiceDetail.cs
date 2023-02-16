namespace CT554_API.Entity
{
	public class InvoiceDetail
	{
		public Guid InvoiceId { get; set; }
		public int ProductDetailId { get; set; }
		public float Quantity { get; set; } = 1.0f;
		public virtual Invoice? Invoice { get; set; }
		public virtual ProductDetail? ProductDetail { get; set; }
	}
}
