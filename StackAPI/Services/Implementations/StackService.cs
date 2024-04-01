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
