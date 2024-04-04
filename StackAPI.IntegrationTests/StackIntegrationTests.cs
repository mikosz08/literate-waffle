using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using StackAPI.DTOs;
using StackAPI.Models;

namespace StackAPI.IntegrationTests
{
    public class StackIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public StackIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/StackTags/LoadTags")]
        [InlineData("/StackTags/RefreshTags")]
        [InlineData("/StackTags/PagedTags")]
        public async Task EndpointIsAvailable(string url)
        {
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task LoadTags_ReturnsJsonWithTags()
        {
            var response = await _client.GetAsync("/StackTags/LoadTags");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var tags = JsonConvert.DeserializeObject<List<StackTagDto>>(jsonString);

            Assert.NotNull(tags);
            Assert.True(tags.Count > 0);
        }

        [Fact]
        public async Task PagedTags_HandlesQueryParamsCorrectly()
        {
            var response = await _client.GetAsync("/StackTags/PagedTags?pageNumber=2&pageSize=10");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonConvert.DeserializeObject<PagedResult<StackTagDto>>(jsonString);

            Assert.NotNull(pagedResult);
            Assert.Equal(2, pagedResult.CurrentPage);
            Assert.Equal(10, pagedResult.PageSize);
            Assert.NotNull(pagedResult.Items);
        }
    }
}
