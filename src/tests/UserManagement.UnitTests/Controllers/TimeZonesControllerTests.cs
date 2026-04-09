using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesAutoComplete;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class TimeZonesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<TimeZonesController>> _mockLogger = new();

        private TimeZonesController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAllTimeZonesAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZonesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TimeZonesDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<TimeZonesDto> { new TimeZonesDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllTimeZonesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZonesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TimeZonesDto>>
                {
                    IsSuccess = true,
                    Message = "No records",
                    Data = new List<TimeZonesDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllTimeZonesAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZonesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TimeZonesDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<TimeZonesDto> { new TimeZonesDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllTimeZonesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetTimeZonesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZoneByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TimeZonesDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetTimeZones_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZonesAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TimeZonesAutoCompleteDto>());

            var result = await CreateSut().GetTimeZones("UTC");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTimeZones_AutoComplete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTimeZonesAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TimeZonesAutoCompleteDto>());

            await CreateSut().GetTimeZones("UTC");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetTimeZonesAutocompleteQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
