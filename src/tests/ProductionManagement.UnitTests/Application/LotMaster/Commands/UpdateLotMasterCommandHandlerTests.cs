using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.UpdateLotMaster;

namespace ProductionManagement.UnitTests.Application.LotMaster.Commands
{
    public sealed class UpdateLotMasterCommandHandlerTests
    {
        private readonly Mock<ILotMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateLotMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int returnId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.LotMaster>(It.IsAny<UpdateLotMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.LotMaster());

            _mockQueryRepo.Setup(r => r.IsLotMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()))
                .ReturnsAsync(returnId);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new UpdateLotMasterCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Lot Master updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new UpdateLotMasterCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.LotMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new UpdateLotMasterCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "LOTMASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWhenLinked_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.IsLotMasterLinkedAsync(1)).ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new UpdateLotMasterCommand { Id = 1, IsActive = 0 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*cannot inactivate*");
        }

        [Fact]
        public async Task Handle_InactivateWhenNotLinked_Succeeds()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new UpdateLotMasterCommand { Id = 1, IsActive = 0 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
