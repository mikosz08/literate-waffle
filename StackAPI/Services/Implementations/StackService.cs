using StackAPI.Data;
using StackAPI.DTOs;
using StackAPI.Models;
using StackAPI.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackAPI.Services.Implementations
{
    public class StackService : IStackService
    {
        private static readonly StackTagSerializer _stackTagSerializer = new("tags.json");
        private readonly HttpClient _httpClient;
        private readonly string? _stackTagUri;
        private readonly string? _site;
        private readonly string? _apiKey;

        public StackService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            _site = config["StackOverflowApi:Site"];
            _stackTagUri = config["StackOverflowApi:BaseUrl"];
            _apiKey = config["SOApi:Key"];
        }

        public async Task<IEnumerable<StackTagDto>> GetStackTagsAsync()
        {
            if (_stackTagSerializer.IsDataFresh())
            {
                return await _stackTagSerializer.LoadTagsAsync();
            }
            return await FetchAndSaveTagsAsync();
        }

        public async Task<IEnumerable<StackTagDto>> ForceRefreshStackTagsAsync()
        {
            return await FetchAndSaveTagsAsync();
        }

        public async Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions)
        {
            var requestUri = CreateURI(pagingOptions);

            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed: {response}");
            }

            StackApiResponse result = await ReadResponse(response);

            var tags = GetTagsWithPercentageAsync(result?.Items ?? new List<StackTag>());

            return new PagedResult<StackTagDto>
            {
                Items = tags,
                TotalItems = tags.Count,
                CurrentPage = pagingOptions.PageNumber,
                PageSize = pagingOptions.PageSize,
            };
        }

        private async Task<IEnumerable<StackTagDto>> FetchAndSaveTagsAsync()
        {
            var allTags = await FetchTagsFromApiAsync();
            var tagsWithPercentage = GetTagsWithPercentageAsync(allTags);
            await _stackTagSerializer.SaveTagsAsync(tagsWithPercentage);
            return tagsWithPercentage;
        }

        private async Task<List<StackTag>> FetchTagsFromApiAsync(int requiredTagsCount = 1000)
        {
            List<StackTag> allTags = new();
            PagingOptions pagingOptions = new PagingOptions();
            bool moreDataAvailable = true;

            while (allTags.Count < requiredTagsCount && moreDataAvailable)
            {
                var requestUri = CreateURI(pagingOptions);

                var response = await _httpClient.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed: {response}");
                }

                StackApiResponse result = await ReadResponse(response);

                if (result.Items == null || !result.Items.Any())
                {
                    moreDataAvailable = false;
                }
                else
                {
                    allTags.AddRange(result.Items);
                    pagingOptions.PageNumber++;
                }
            }
            return allTags;
        }

        private static async Task<StackApiResponse> ReadResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StackApiResponse>(content, GetJsonOptions());
            if (result == null)
            {
                return new StackApiResponse();
            }
            return result;
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

        private static List<StackTagDto> GetTagsWithPercentageAsync(IEnumerable<StackTag> tags)
        {
            if (tags.Count() < 0)
            {
                return new List<StackTagDto>();
            }
            var totalTagCount = tags.Sum(tag => tag.Count);

            List<StackTagDto> result = tags.Select(tag => new StackTagDto
            {
                Name = tag.Name,
                Count = tag.Count,
                Popular = (double)tag.Count / totalTagCount * 100
            }).ToList();

            return result;
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