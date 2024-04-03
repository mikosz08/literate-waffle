using Microsoft.AspNetCore.Mvc;
using StackAPI.Models;
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

        [HttpGet("LoadTags")]
        public async Task<IActionResult> LoadTags()
        {
            var tags = await _stackService.GetStackTagsAsync();
            return Ok(tags);
        }

        [HttpGet("RefreshTags")]
        public async Task<IActionResult> RefreshTags()
        {
            var tags = await _stackService.ForceRefreshStackTagsAsync();
            return Ok(tags);
        }

        [HttpGet("PagedTags")]
        public async Task<IActionResult> GetPagedTags([FromQuery] PagingOptions options)
        {
            var tags = await _stackService.GetStackTagsPagedAsync(options);
            return Ok(tags);
        }

    }
}
