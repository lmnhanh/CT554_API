namespace CT554_API.Entity
{
	public class Vender
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string? Company { get; set; }
		public string? Desciption { get; set; }

		public virtual ICollection<Invoice>? Invoices { get; set; }
	}
}
