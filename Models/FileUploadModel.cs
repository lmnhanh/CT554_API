using Microsoft.VisualBasic.FileIO;

namespace CT554_API.Models
{
    public class FileUploadModel
    {
        public IFormFile File { get; set; } = null!;
    }
}
