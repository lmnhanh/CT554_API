namespace CT554_API.Models
{
    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime DateUpdate { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }
}
