namespace CT554_API.Entity
{
	public class ProductDetail
	{
		public int Id { get; set; }
		public string Unit { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int ToWholesale { get; set; } = 1;
		public bool isAvailable { get; set; } = true;

		public int ProductId { get; set; }
		public virtual Product? Product { get; set; }
		public virtual ICollection<Price>? Prices { get; set; }
		public virtual ICollection<Stock>? Stocks { get; set; }
		public virtual ICollection<Cart>? Carts { get; set; }
		public virtual ICollection<InvoiceDetail>? InvoiceDetails { get; set; }
	}
}
