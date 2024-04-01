using StackAPI.DTOs;
using StackAPI.Models;

namespace StackAPI.Services.Interfaces
{
    public interface IStackService
    {
        Task<IEnumerable<StackTag>> GetStackTagsAsync();
        Task<IEnumerable<StackTagDto>> GetStackTagsWithPercentageAsync();
    }
}
