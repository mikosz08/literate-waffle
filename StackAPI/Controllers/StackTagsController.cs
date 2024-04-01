using Microsoft.AspNetCore.Mvc;
using StackAPI.Services.Interfaces;

namespace StackAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StackTagsController : ControllerBase
    {
        private readonly IStackService _stackService;

        public StackTagsController(IStackService stackService)
        {
            _stackService = stackService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tags = await _stackService.GetStackTagsWithPercentageAsync();
            return Ok(tags);
        }

    }
}
