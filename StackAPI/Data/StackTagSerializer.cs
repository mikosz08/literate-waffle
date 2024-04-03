using StackAPI.DTOs;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StackAPI.Data
{
    public class StackTagSerializer
    {
        private readonly string _filePath;
        private JsonSerializerOptions _options;

        public StackTagSerializer(string filePath)
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
        }

        public bool IsDataFresh()
        {
            var freshHours = 24;
            if (!File.Exists(_filePath))
            {
                return false;
            }

            var lastWriteTime = File.GetLastWriteTime(_filePath);
            return (DateTime.Now - lastWriteTime).TotalHours <= freshHours;
        }

        public async Task<(DateTime LastModified, int TagCount)> GetFileInfoAsync()
        {
            if (!File.Exists(_filePath))
            {
                return (DateTime.MinValue, 0);
            }

            var lastWriteTime = File.GetLastWriteTime(_filePath);
            var tags = await LoadTagsAsync();
            int tagCount = tags.Count;

            return (lastWriteTime, tagCount);
        }

        public async Task SaveTagsAsync(IEnumerable<StackTagDto> tags)
        {
            var json = JsonSerializer.Serialize(tags, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<StackTagDto>> LoadTagsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<StackTagDto>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<StackTagDto>>(json, _options) ?? new List<StackTagDto>();
        }
    }
}
