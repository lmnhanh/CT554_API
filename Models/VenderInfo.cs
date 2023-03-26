namespace CT554_API.Models
{
    public class VenderInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Company { get; set; }
        public string? Description { get; set; }
        public DateTime DateStart { get; set; } = DateTime.UtcNow;

        public int InvoiceCount { get; set; }
    }
}
