namespace CT554_API.Models
{
    public class InvoiceDTO
    {
        public Guid Id { get; set; }
        public Guid VenderId { get; set; }
        public float RealTotal { get; set; } = 1f;
        public List<InvoiceDetailDTO> InvoiceDetails { get; set; } = null!;
    }
}
