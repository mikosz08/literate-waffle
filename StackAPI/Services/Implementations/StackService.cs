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
        private readonly ILogger<StackService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string? _stackTagUri;
        private readonly string? _site;
        private readonly string? _apiKey;

        public StackService(HttpClient httpClient, IConfiguration config, ILogger<StackService> logger)
        {
            _httpClient = httpClient;
            _site = config["StackOverflowApi:Site"];
            _stackTagUri = config["StackOverflowApi:BaseUrl"];
            _apiKey = config["StackOverflowApi:Key"];
            _logger = logger;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("StackAPI/1.0");
            _logger.LogInformation("Stack Service initialized.");
        }

        public async Task<IEnumerable<StackTagDto>> GetStackTagsAsync()
        {
            _logger.LogInformation("Proceeding to get StackData");
            if (_stackTagSerializer.IsDataFresh())
            {
                _logger.LogInformation("The Tags data is fresh. Reading from a file.");
                return await _stackTagSerializer.LoadTagsAsync();
            }
            _logger.LogInformation("The Tags data is empty or old. Fetching from StackOverflow.");
            return await FetchAndSaveTagsAsync();
        }

        public async Task<IEnumerable<StackTagDto>> ForceRefreshStackTagsAsync()
        {
            _logger.LogInformation("Forced refresh of StackTags initiated.");
            return await FetchAndSaveTagsAsync();
        }

        public async Task<PagedResult<StackTagDto>> GetStackTagsPagedAsync(PagingOptions pagingOptions)
        {
            _logger.LogInformation("Proceeding to get paged StackData with PageNumber: {PageNumber}, PageSize: {PageSize}", pagingOptions.PageNumber, pagingOptions.PageSize);
            var requestUri = CreateURI(pagingOptions);

            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request failed with status code: {StatusCode} and URI: {RequestUri}", response.StatusCode, requestUri);
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
            _logger.LogInformation("Fetching tags from StackOverflow.");
            var allTags = await FetchTagsFromApiAsync();
            var tagsWithPercentage = GetTagsWithPercentageAsync(allTags);

            _logger.LogInformation("Saving {Count} tags.", tagsWithPercentage.Count);
            await _stackTagSerializer.SaveTagsAsync(tagsWithPercentage);
            return tagsWithPercentage;
        }

        private async Task<List<StackTag>> FetchTagsFromApiAsync(int requiredTagsCount = 1000)
        {
            _logger.LogInformation("Fetching up to {RequiredTagsCount} tags.", requiredTagsCount);
            List<StackTag> allTags = new();
            PagingOptions pagingOptions = new PagingOptions();
            bool moreDataAvailable = true;

            while (allTags.Count < requiredTagsCount && moreDataAvailable)
            {
                var requestUri = CreateURI(pagingOptions);

                var response = await _httpClient.GetAsync(requestUri);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Request failed with status code: {StatusCode}", response.StatusCode);
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
            _logger.LogInformation("Fetched {Count} tags.", allTags.Count);
            return allTags;
        }

        private static async Task<StackApiResponse> ReadResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StackApiResponse>(content, GetJsonOptions());
            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize API response.");
            }
            return result;
        }

        private string CreateURI(PagingOptions pagingOptions)
        {
            var encodedApiKey = Uri.EscapeDataString(_apiKey ?? string.Empty);

            var uri = $"{_stackTagUri}?" +
                    $"key={encodedApiKey}&" +
                    $"page={pagingOptions.PageNumber}&" +
                    $"pagesize={pagingOptions.PageSize}&" +
                    $"order={pagingOptions.Order}&" +
                    $"sort={pagingOptions.Sort}&" +
                    $"site={_site}";
            return uri;
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