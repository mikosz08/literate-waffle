using StackAPI.DTOs;
using StackAPI.Models;

namespace StackAPI.Services.Interfaces
{
    public interface IStackService
    {
        Task<IEnumerable<StackTagDto>> GetStackTagsAsync();
        Task<IEnumerable<StackTagDto>> ForceRefreshStackTagsAsync();
        Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions);
    }
}
