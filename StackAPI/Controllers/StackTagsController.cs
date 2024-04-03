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
        private readonly ILogger<StackTagsController> _logger;

        public StackTagsController(IStackService stackService, ILogger<StackTagsController> logger)
        {
            _stackService = stackService;
            _logger = logger;
        }

        [HttpGet("LoadTags")]
        public async Task<IActionResult> LoadTags()
        {
            try
            {
                _logger.LogInformation("Loading tags");
                var tags = await _stackService.GetStackTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("RefreshTags")]
        public async Task<IActionResult> RefreshTags()
        {
            try
            {
                _logger.LogInformation("Refreshing tags");
                var tags = await _stackService.ForceRefreshStackTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing tags");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("PagedTags")]
        public async Task<IActionResult> GetPagedTags([FromQuery] PagingOptions options)
        {
            try
            {
                _logger.LogInformation("Getting paged tags");
                var tags = await _stackService.GetStackTagsPagedAsync(options);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged tags");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
