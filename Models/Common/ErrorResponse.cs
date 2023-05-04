using System.Text.Json;

namespace CT554_API.Models.Common
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;
        public List<string> errors { get; set; } = new List<string>();
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
