using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StackAPI.Controllers;
using StackAPI.DTOs;
using StackAPI.Models;
using StackAPI.Services.Interfaces;

namespace StackAPI.UnitTests
{
    public class StackUnitTests
    {
        [Fact]
        public async Task LoadTags_ReturnsOkObjectResult_WithTags()
        {
            var mockService = new Mock<IStackService>();
            var testTags = new List<StackTagDto> { new StackTagDto { Name = "test", Count = 1, Popular = 0.5 } };
            var mockLogger = new Mock<ILogger<StackTagsController>>();

            mockService.Setup(service => service.GetStackTagsAsync()).ReturnsAsync(testTags);

            var controller = new StackTagsController(mockService.Object, mockLogger.Object);

            var result = await controller.LoadTags();

            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<List<StackTagDto>>(actionResult.Value);
            Assert.Single(returnedTags);
            Assert.Equal("test", returnedTags[0].Name);

        }

        [Fact]
        public async Task RefreshTags_ReturnsOkObjectResult_WithTags()
        {

            var mockService = new Mock<IStackService>();
            var testTags = new List<StackTagDto> { new StackTagDto { Name = "test", Count = 1, Popular = 0.5 } };
            var mockLogger = new Mock<ILogger<StackTagsController>>();

            mockService.Setup(service => service.ForceRefreshStackTagsAsync()).ReturnsAsync(testTags);

            var controller = new StackTagsController(mockService.Object, mockLogger.Object);

            var result = await controller.RefreshTags();

            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<List<StackTagDto>>(actionResult.Value);
            Assert.Single(returnedTags);
            Assert.Equal("test", returnedTags[0].Name);
        }

        [Fact]
        public async Task GetPagedTags_ReturnsOkObjectResult_WithTags()
        {

            var mockService = new Mock<IStackService>();
            var mockLogger = new Mock<ILogger<StackTagsController>>();

            var testTags = new List<StackTagDto> { new StackTagDto { Name = "test", Count = 1, Popular = 0.5 } };
            var options = new PagingOptions();
            var pagedTags = new PagedResult<StackTagDto>
            {
                Items = testTags,
                TotalItems = testTags.Count,
                CurrentPage = options.PageNumber,
                PageSize = options.PageSize,
            };

            mockService.Setup(service => service.GetStackTagsPagedAsync(options)).ReturnsAsync(pagedTags);

            var controller = new StackTagsController(mockService.Object, mockLogger.Object);

            var result = await controller.GetPagedTags(options);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedTags = Assert.IsType<PagedResult<StackTagDto>>(actionResult.Value);

            Assert.Equal(1, returnedTags.TotalItems);
            Assert.Equal(1, returnedTags.CurrentPage);
            Assert.Equal(100, returnedTags.PageSize);

            var totalPages = (int)Math.Ceiling((double)returnedTags.TotalItems / returnedTags.PageSize);
            Assert.Equal(totalPages, returnedTags.TotalPages);

            var returnedTagsList = returnedTags.Items?.ToList() ?? new List<StackTagDto>();
            Assert.Single(returnedTagsList);
            Assert.Equal("test", returnedTagsList[0].Name);
        }

        [Fact]
        public async Task GetPagedTags_ReturnsInternalServerError_WhenServiceThrowsException()
        {
            var mockService = new Mock<IStackService>();
            var mockLogger = new Mock<ILogger<StackTagsController>>();

            var options = new PagingOptions();

            mockService.Setup(service => service.GetStackTagsPagedAsync(options)).ThrowsAsync(new Exception("Test exception"));

            var controller = new StackTagsController(mockService.Object, mockLogger.Object);

            var result = await controller.GetPagedTags(options);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

    }
}
