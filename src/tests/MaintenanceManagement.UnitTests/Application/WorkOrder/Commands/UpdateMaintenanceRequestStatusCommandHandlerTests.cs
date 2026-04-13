using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UpdateMaintenanceRequestStatusCommandHandlerTests
    {
        private readonly Mock<IMaintenanceRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private UpdateMaintenanceRequestStatusCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object);

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsSuccessResponse()
        {
            _mockCommandRepo.Setup(r => r.UpdateStatusAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new UpdateMaintenanceRequestStatusCommand { Id = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Contain("successfully");
        }

        [Fact]
        public async Task Handle_FailedUpdate_ReturnsFailureResponse()
        {
            _mockCommandRepo.Setup(r => r.UpdateStatusAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateSut().Handle(
                new UpdateMaintenanceRequestStatusCommand { Id = 99 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
            result.Message.Should().Contain("failed");
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockCommandRepo.Setup(r => r.UpdateStatusAsync(It.IsAny<int>())).ReturnsAsync(true);

            await CreateSut().Handle(
                new UpdateMaintenanceRequestStatusCommand { Id = 5 },
                CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateStatusAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_PassesCorrectId()
        {
            _mockCommandRepo.Setup(r => r.UpdateStatusAsync(42)).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new UpdateMaintenanceRequestStatusCommand { Id = 42 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockCommandRepo.Verify(r => r.UpdateStatusAsync(42), Times.Once);
        }
    }
}
