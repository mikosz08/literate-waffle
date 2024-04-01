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

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _stackService.GetStackTagsAsync();
            return Ok(tags);
        }

        [HttpGet("All_Percentage")]
        public async Task<IActionResult> GetAllWithPercentage()
        {
            var tags = await _stackService.GetStackTagsWithPercentageAsync();
            return Ok(tags);
        }

        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortDirection = "ASC")
        {
            var pagingOptions = new PagingOptions
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var tags = await _stackService.GetStackTagsPagedAsync(pagingOptions);

            return Ok(tags);
        }


    }
}
