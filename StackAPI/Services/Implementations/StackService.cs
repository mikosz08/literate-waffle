using StackAPI.DTOs;
using StackAPI.Models;
using StackAPI.Services.Interfaces;
using System.Text.Json;

namespace StackAPI.Services.Implementations
{
    public class StackService : IStackService
    {

        private readonly HttpClient _httpClient;

        private readonly string stackTagUri = "https://api.stackexchange.com/2.2/tags?order=desc&sort=popular&site=stackoverflow";

        public StackService(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }

        public async Task<IEnumerable<StackTagDto>> GetStackTagsWithPercentageAsync()
        {
            var tags = await GetStackTagsAsync();
            var totalTagCount = tags.Sum(tag => tag.Count);

            return tags.Select(tag => new StackTagDto
            {
                Name = tag.Name,
                Count = tag.Count,
                Percentage = (double)tag.Count / totalTagCount * 100
            }).ToList();
        }

        public async Task<IEnumerable<StackTag>> GetStackTagsAsync()
        {
            var response = await _httpClient.GetAsync(stackTagUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StackApiResponse>(content, GetJsonOptions());

            return result?.Items ?? new List<StackTag>();
        }


        public async Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions)
        {
            var tags = await GetStackTagsWithPercentageAsync();

            IEnumerable<StackTagDto> sortedTags = pagingOptions.SortDirection?.ToUpper() == "ASC" ?
            (pagingOptions.SortBy == "Name" ? tags.OrderBy(t => t.Name) : tags.OrderBy(t => t.Percentage)) :
            (pagingOptions.SortBy == "Name" ? tags.OrderByDescending(t => t.Name) : tags.OrderByDescending(t => t.Percentage));

            var skipCount = (pagingOptions.PageNumber - 1) * pagingOptions.PageSize;

            var pagedTags = sortedTags.Skip(skipCount).Take(pagingOptions.PageSize);

            return new PagedResult<StackTagDto>
            {
                Items = pagedTags.ToList(),
                TotalItems = tags.Count(),
                CurrentPage = pagingOptions.PageNumber,
                PageSize = pagingOptions.PageSize
            };
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return options;
        }

    }
}
