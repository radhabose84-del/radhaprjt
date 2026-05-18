using MaintenanceManagement.Application.Common.Interfaces.IServiceHistory;
using MaintenanceManagement.Application.ServiceHistory.Dto;
using MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ServiceHistory.Queries
{
    public sealed class GetServiceHistoryQueryHandlerTests
    {
        private readonly Mock<IServiceHistoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetServiceHistoryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        private void SetupRepo(List<ServiceHistoryDto> items, int total)
        {
            _mockQueryRepo
                .Setup(r => r.GetServiceHistoryAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((items, total));
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccessAndData()
        {
            var items = new List<ServiceHistoryDto>
            {
                new() { EventType = "WorkOrder", SourceId = 10, MachineId = 5, DocNo = "WO-1" }
            };
            SetupRepo(items, 1);

            var result = await CreateSut().Handle(
                new GetServiceHistoryQuery { MachineId = 5, PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsSuccessWithEmptyData()
        {
            SetupRepo(new List<ServiceHistoryDto>(), 0);

            var result = await CreateSut().Handle(
                new GetServiceHistoryQuery { AssetId = 99 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.Message.Should().Be("No service history found.");
        }

        [Fact]
        public async Task Handle_CallsRepository_Once()
        {
            SetupRepo(new List<ServiceHistoryDto>(), 0);

            await CreateSut().Handle(
                new GetServiceHistoryQuery { MachineId = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetServiceHistoryAsync(
                It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupRepo(new List<ServiceHistoryDto>(), 0);

            await CreateSut().Handle(
                new GetServiceHistoryQuery { MachineId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.Module == "ServiceHistory"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
