using Microsoft.AspNetCore.Mvc;
using StackAPI.Models;
using StackAPI.Services.Interfaces;

namespace StackAPI.Controllers
{
    /// <summary>
    /// Controller for managing Stack Overflow tags.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StackTagsController : ControllerBase
    {
        private readonly IStackService _stackService;
        private readonly ILogger<StackTagsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackTagsController"/> class.
        /// </summary>
        /// <param name="stackService">The stack service.</param>
        /// <param name="logger">The logger.</param>
        public StackTagsController(IStackService stackService, ILogger<StackTagsController> logger)
        {
            _stackService = stackService;
            _logger = logger;
        }

        /// <summary>
        /// Loads all Stack Overflow tags.
        /// </summary>
        /// <returns>An IActionResult containing all tags.</returns>
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

        /// <summary>
        /// Refreshes and loads all Stack Overflow tags.
        /// </summary>
        /// <returns>An IActionResult containing refreshed tags.</returns>
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

        /// <summary>
        /// Gets paged Stack Overflow tags based on the provided options.
        /// </summary>
        /// <param name="options">Paging and sorting options.</param>
        /// <returns>An IActionResult containing paged tags.</returns>
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
