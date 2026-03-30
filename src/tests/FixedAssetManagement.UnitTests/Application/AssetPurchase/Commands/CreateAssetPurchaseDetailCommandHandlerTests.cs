using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Commands
{
    public sealed class CreateAssetPurchaseDetailCommandHandlerTests
    {
        private readonly Mock<IAssetPurchaseCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateAssetPurchaseDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int returnId)
        {
            var entity = AssetPurchaseBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<AssetPurchaseDetails>(It.IsAny<CreateAssetPurchaseDetailCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetPurchaseDetails>()))
                .ReturnsAsync(returnId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);

            var result = await CreateSut().Handle(AssetPurchaseBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(AssetPurchaseBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<AssetPurchaseDetails>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(AssetPurchaseBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            SetupHappyPath(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(AssetPurchaseBuilders.ValidCreateCommand(), CancellationToken.None));
        }
    }
}
