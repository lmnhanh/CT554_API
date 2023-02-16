using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CT554_API.Entity
{
	public class User : IdentityUser
	{
		public string FullName { get; set; } = string.Empty;
		public bool Gender { get; set; } = true; //Nam
		public DateTime? DayOfBirth { get; set; }

		public ICollection<Cart>? Carts { get; set; }
		public ICollection<Order>? Orders { get; set; }
	}
}
