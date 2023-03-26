namespace CT554_API.Models
{
    public class InvoiceInfo
    {
        public Guid Id { get; set; }
		public DateTime DateCreate { get; set; }
        public float RealTotal { get; set; } = 1f;
        public VenderInfo Vender { get; set; } = null!;
        public List<InvoiceDetailInfo> InvoiceDetails { get; set; } = null!;
    }
}
