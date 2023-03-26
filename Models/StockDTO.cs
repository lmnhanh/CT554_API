namespace CT554_API.Models
{
    public class StockDTO
    {
        public int ProductDetailId { get; set; }
        public bool IsManualUpdate { get; set; } = false;
        public float Value { get; set; } = 1.0f;
    }
}
