using StackAPI.DTOs;
using StackAPI.Models;

namespace StackAPI.Services.Interfaces
{
    public interface IStackService
    {
        Task<IEnumerable<StackTagDto>> GetStackTagsAsync();
        Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions);
    }
}
