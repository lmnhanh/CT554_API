namespace CT554_API.Models.View
{
    public class StockCombineModel
    {
        public object? Id { get; set; }
        public DateTime DateUpdate { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "";   //invoice, order, manual update
        public string InvoiceId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public float Value { get; set; } = 0;
        public string? Description { get; set; }
    }
}
