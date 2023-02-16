namespace CT554_API.Models.Auth
{
	public class ConnectModel
	{
		public string access_token { get; set; } = null!;
		public int expires_in { get; set; } = 3600;
		public string token_type { get; set; } = null!;
		public string scope { get; set; } = null!;
	}
}
