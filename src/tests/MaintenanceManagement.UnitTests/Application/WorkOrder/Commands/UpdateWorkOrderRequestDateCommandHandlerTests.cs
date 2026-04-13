using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UpdateWorkOrderRequestDateCommandHandlerTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private UpdateWorkOrderRequestDateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object);

        private static UpdateWorkOrderRequestDateCommand ValidCommand() => new()
        {
            WorkOrderId = 1,
            RequestDate = DateTimeOffset.UtcNow,
            IsSystemTime = 1
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateRequestDateAsync(
                    It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateRequestDateAsync(
                    It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateRequestDateAsync(
                It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RepoReturnsFalse_ReturnsFalse()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateRequestDateAsync(
                    It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PassesCorrectWorkOrderId()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateRequestDateAsync(
                    42, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = ValidCommand();
            command.WorkOrderId = 42;
            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateRequestDateAsync(
                42, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
