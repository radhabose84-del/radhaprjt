using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAdditionalCost.Commands
{
    public sealed class CreateAssetAdditionalCostCommandHandlerTests
    {
        private readonly Mock<IAssetAdditionalCostCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateAssetAdditionalCostCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = AssetAdditionalCostBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var command = AssetAdditionalCostBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = AssetAdditionalCostBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetAdditionalCostBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var entity = AssetAdditionalCostBuilders.ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()))
                .ReturnsAsync(0);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(AssetAdditionalCostBuilders.ValidCreateCommand(), CancellationToken.None));
        }
    }
}
