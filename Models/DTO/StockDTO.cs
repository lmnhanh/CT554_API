namespace CT554_API.Models.DTO
{
    public class StockDTO
    {
        public int ProductDetailId { get; set; }
        public string? Description { get; set; }
        public bool IsManualUpdate { get; set; } = false;
        public float Value { get; set; } = 1.0f;
    }
}
