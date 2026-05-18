using Contracts.Common;
using MaintenanceManagement.Application.ServiceHistory.Dto;
using MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class ServiceHistoryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ServiceHistoryController CreateSut() => new(_mockMediator.Object);

        private void SetupSend(int totalCount = 1)
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceHistoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ServiceHistoryDto>>
                {
                    IsSuccess = true,
                    Data = new List<ServiceHistoryDto> { new() { EventType = "WorkOrder" } },
                    TotalCount = totalCount,
                    PageNumber = 1,
                    PageSize = 10
                });
        }

        [Fact]
        public async Task GetServiceHistory_ReturnsOkResult()
        {
            SetupSend();

            var result = await CreateSut().GetServiceHistoryAsync(5, null, null, null, 1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetServiceHistory_CallsMediatorSend_Once()
        {
            SetupSend();

            await CreateSut().GetServiceHistoryAsync(null, 7, null, null, 1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetServiceHistoryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
