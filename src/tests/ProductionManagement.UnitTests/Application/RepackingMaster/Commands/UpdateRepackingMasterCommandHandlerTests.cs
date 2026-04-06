using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;

namespace ProductionManagement.UnitTests.Application.RepackingMaster.Commands
{
    public sealed class UpdateRepackingMasterCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper
                .Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<UpdateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateRepackingHeaderCommand { Id = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            SetupHappyPath(result: 5);
            var result = await CreateSut().Handle(new UpdateRepackingHeaderCommand { Id = 5 }, CancellationToken.None);
            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRepackingHeaderCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRepackingHeaderCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "REPACKING_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_UsesMapper()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRepackingHeaderCommand { Id = 1 }, CancellationToken.None);
            _mockMapper.Verify(
                m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<UpdateRepackingHeaderCommand>()),
                Times.Once);
        }
    }
}
