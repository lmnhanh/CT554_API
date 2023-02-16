namespace CT554_API.Entity
{
	public class Invoice
	{
		public Guid Id { get; set; }
		public DateTime DateCreate { get; set; } = DateTime.UtcNow;
		public virtual ICollection<InvoiceDetail>? Details { get; set; }
	}
}
