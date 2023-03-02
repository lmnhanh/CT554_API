namespace CT554_API.Entity
{
	public class Image
	{
		public string URL { get; set; } = string.Empty;
		public byte[] Content { get; set; } = Array.Empty<byte>();
		public int? ProductId { get; set; }
		public virtual Product? Product { get; set; }
	}
}
