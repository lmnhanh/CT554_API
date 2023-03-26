namespace CT554_API.Models
{
	public static class PaginationResponse<T>
	{
		public static List<T> List { get; set; } = new List<T>();
	}
}
