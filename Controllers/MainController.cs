using AdWebService.DTO;
using AdWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : Controller
    {
        private readonly IMainService _mainService;

        public MainController(IMainService mainService)
        {
            _mainService = mainService;
        }

        [HttpPost("load")]
        public async Task<ActionResult<ApiResponse>> LoadFile()
        {
            Console.WriteLine("=== LOAD FILE METHOD CALLED ==="); // Add this

            var result = _mainService.LoadPlatforms();
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("search/{path}")]
        public async Task<ActionResult<ApiResponse>> Search(string path)
        {
            Console.WriteLine($"=== SEARCH METHOD CALLED with path: '{path}' ==="); // Add this

            var result = _mainService.FindPlatformsByPath(path);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
    }
}
