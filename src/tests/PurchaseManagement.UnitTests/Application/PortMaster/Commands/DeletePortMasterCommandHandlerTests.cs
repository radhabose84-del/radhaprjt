using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Commands.Delete;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PortMaster.Commands
{
    public sealed class DeletePortMasterCommandHandlerTests
    {
        private readonly Mock<IPortMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeletePortMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = PortMasterBuilders.ValidDto(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new DeletePortMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PortMasterDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(new DeletePortMasterCommand(999), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PortMasterBuilders.ValidDto());

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeletePortMasterCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSoftDeleteOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeletePortMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeletePortMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
