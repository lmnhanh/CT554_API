using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(policy: "Admin")]
    //[Authorize(Roles ="Admin")]
    public class PromotionController : ControllerBase
    {
    }
}
