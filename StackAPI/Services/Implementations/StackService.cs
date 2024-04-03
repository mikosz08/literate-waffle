using StackAPI.DTOs;
using StackAPI.Models;
using System.Text.Json;
using StackAPI.Services.Interfaces;
using System.Text.Json.Serialization;

namespace StackAPI.Services.Implementations
{
    public class StackService : IStackService
    {

        private readonly HttpClient _httpClient;
        private readonly string? _stackTagUri;
        private readonly string? _site;
        private readonly string? _apiKey;

        public StackService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _site = config["StackOverflowApi:Site"];
            _apiKey = config["SOApi:Key"];
            _stackTagUri = config["StackOverflowApi:BaseUrl"];
        }

        public async Task<IEnumerable<StackTagDto>> GetStackTagsAsync()
        {
            List<StackTag> allTags = new();
            PagingOptions pagingOptions = new();
            bool moreDataAvailable = true;

            const int requiredTagsCount = 1000;

            while (allTags.Count < requiredTagsCount && moreDataAvailable)
            {
                var requestUri = CreateURI(pagingOptions);

                var response = await _httpClient.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed: {response}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StackApiResponse>(content, GetJsonOptions());

                if (result?.Items == null || !result.Items.Any())
                {
                    moreDataAvailable = false;
                }
                else
                {
                    allTags.AddRange(result.Items);
                    pagingOptions.PageNumber++;
                }
            }
            return GetTagsWithPercentage(allTags);
        }


        public async Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions)
        {
            var requestUri = CreateURI(pagingOptions);
            Console.WriteLine(requestUri);

            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {response}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StackApiResponse>(content, GetJsonOptions());

            var tags = GetTagsWithPercentage(result?.Items ?? new List<StackTag>());

            return new PagedResult<StackTagDto>
            {
                Items = tags,
                TotalItems = tags.Count,
                CurrentPage = pagingOptions.PageNumber,
                PageSize = pagingOptions.PageSize,
            };
        }

        private string CreateURI(PagingOptions pagingOptions)
        {
            return $"{_stackTagUri}?" +
                $"key={_apiKey}&" +
                $"page={pagingOptions.PageNumber}&" +
                $"pagesize={pagingOptions.PageSize}&" +
                $"order={pagingOptions.Order}&" +
                $"sort={pagingOptions.Sort}&" +
                $"site={_site}";
        }

        private static List<StackTagDto> GetTagsWithPercentage(IEnumerable<StackTag> tags)
        {
            if (tags.Count() < 0)
            {
                return new List<StackTagDto>();
            }
            var totalTagCount = tags.Sum(tag => tag.Count);

            return tags.Select(tag => new StackTagDto
            {
                Name = tag.Name,
                Count = tag.Count,
                Popular = (double)tag.Count / totalTagCount * 100
            }).ToList();
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            return options;
        }

    }
}