using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Commands
{
    public sealed class UpdateAssetPurchaseDetailCommandHandlerTests
    {
        private readonly Mock<IAssetPurchaseCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetPurchaseQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetPurchaseDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private static UpdateAssetPurchaseDetailCommand ValidUpdateCommand() =>
            new UpdateAssetPurchaseDetailCommand
            {
                Id = 1,
                ItemName = "Updated Item",
                ItemCode = "ITM001",
                GrnNo = 1001,
                PoNo = 2001,
                PurchaseValue = 60000m
            };

        private void SetupHappyPath(int updatedRows = 1)
        {
            var existing = new AssetPurchaseDetails { Id = 1, ItemName = "Old Item" };
            var mapped = new AssetPurchaseDetails { Id = 1, ItemName = "Updated Item" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<AssetPurchaseDetails>(It.IsAny<object>()))
                .Returns(mapped);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetPurchaseDetails>()))
                .ReturnsAsync(updatedRows);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsAffectedRows()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetPurchaseDetails>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AssetPurchaseDetails?)null);

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsValidationException()
        {
            var existing = new AssetPurchaseDetails { Id = 1 };
            var mapped = new AssetPurchaseDetails { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<AssetPurchaseDetails>(It.IsAny<object>()))
                .Returns(mapped);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<AssetPurchaseDetails>()))
                .ReturnsAsync(0);

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }
    }
}
