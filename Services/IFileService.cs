using CT554_API.Data;
using CT554_API.Entity;

namespace CT554_API.Services
{
    public interface IFileService
    {
        public Task PostFileAsync(IFormFile fileData);
        public Task PostMultiFileAsync(List<IFormFile> fileData);
    }
    public class FileService : IFileService
    {
        private readonly CT554DbContext _context;

        public FileService(CT554DbContext context)
        {
            _context = context;
        }

        public async Task PostFileAsync(IFormFile file)
        {
            try
            {
                string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
                var image = new Image();
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    image.URL = $"{file.Length}_{file.GetHashCode()}";
                    image.Content = stream.ToArray();

                    //string randomName = Path.GetRandomFileName();
                    //string fileName = randomName.Substring(0, randomName.Length - 4) + image.FileName;
                }

                var result = _context.Images.Add(image);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task PostMultiFileAsync(List<IFormFile> files)
        {
            try
            {
                foreach (IFormFile file in files)
                {
                    var image = new Image();

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        image.URL = $"{file.Length}_{file.GetHashCode()}";
                        image.Content = stream.ToArray();
                    }

                    await _context.Images.AddAsync(image);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
