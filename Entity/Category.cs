using CT554_API.Models;
using System.Text.Json.Serialization;

namespace CT554_API.Entity
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime DateUpdate { get; set; } = DateTime.UtcNow;
		public bool IsActive { get; set; } = true;

		[JsonIgnore]
		public virtual ICollection<Product>? Products { get; set; }
	}
}
