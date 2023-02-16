namespace CT554_API.Entity
{
	public class Order
	{
		public Guid Id { get; set; }
		public DateTime DateCreate { get; set; } = DateTime.UtcNow;
		public string Description { get; set; } = string.Empty;
		public bool IsProccesed { get; set; } = false;
		public bool IsSuccess { get; set; } = false;

		public string? UserId { get; set; }
		public virtual User? User { get; set; }

		public ICollection<Cart>? Carts { get; set; }
	}
}
