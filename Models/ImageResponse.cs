using Microsoft.VisualBasic.FileIO;

namespace CT554_API.Models
{
    public class ImageResponse
    {
        public string Name { get; set; } = null!;
        public byte[] Data { get; set; } = null!;
    }
}
