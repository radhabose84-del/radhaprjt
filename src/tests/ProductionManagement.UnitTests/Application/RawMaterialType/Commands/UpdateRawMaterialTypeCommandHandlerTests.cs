using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;

namespace ProductionManagement.UnitTests.Application.RawMaterialType.Commands
{
    public sealed class UpdateRawMaterialTypeCommandHandlerTests
    {
        private readonly Mock<IRawMaterialTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRawMaterialTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int rowsAffected = 1, bool isLinked = false)
        {
            _mockQueryRepo.Setup(r => r.IsRawMaterialTypeLinkedAsync(It.IsAny<int>())).ReturnsAsync(isLinked);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RawMaterialType>(It.IsAny<UpdateRawMaterialTypeCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RawMaterialType());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RawMaterialType>()))
                .ReturnsAsync(rowsAffected);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateRawMaterialTypeCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRawMaterialTypeCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RawMaterialType>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Inactivate_When_NotLinked_Succeeds()
        {
            SetupHappyPath(isLinked: false);
            var result = await CreateSut().Handle(new UpdateRawMaterialTypeCommand { Id = 1, IsActive = 0 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Inactivate_When_Linked_ThrowsExceptionRules()
        {
            SetupHappyPath(isLinked: true);

            Func<Task> act = async () => await CreateSut().Handle(
                new UpdateRawMaterialTypeCommand { Id = 1, IsActive = 0 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked with other records*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateRawMaterialTypeCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RAWMATERIALTYPE_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
