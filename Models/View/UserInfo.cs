namespace CT554_API.Models.View
{
    public class UserInfo
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateAsPartner { get; set; }
        public bool Gender { get; set; } = true;
        public DateTime? DayOfBirth { get; set; }
    }
}
