using FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Commands
{
    public sealed class MapScheduleIIILineCommandHandlerTests
    {
        private readonly Mock<IAccountGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MapScheduleIIILineCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int returnedId = 3)
        {
            _mockCommandRepo
                .Setup(r => r.MapScheduleIIILineAsync(It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(returnedId);
        }

        [Fact]
        public async Task Handle_Map_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("mapped");
        }

        [Fact]
        public async Task Handle_Unmap_ReturnsRemovedMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = null }, CancellationToken.None);

            result.Message.Should().Contain("removed");
        }

        [Fact]
        public async Task Handle_CallsRepoWithSuppliedValues()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.MapScheduleIIILineAsync(3, 10), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "ACCOUNT_GROUP_SCHEDULE_III_MAP"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
