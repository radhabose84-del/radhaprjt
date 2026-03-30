using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAdditionalCost.Commands
{
    public sealed class UpdateAssetAdditionalCostCommandHandlerTests
    {
        private readonly Mock<IAssetAdditionalCostCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetAdditionalCostCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetAdditionalCostBuilders.ValidEntity(id);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            SetupHappyPath(1);
            var command = AssetAdditionalCostBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = AssetAdditionalCostBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsValidationException()
        {
            var entity = AssetAdditionalCostBuilders.ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()))
                .ReturnsAsync(0);

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(AssetAdditionalCostBuilders.ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetAdditionalCostBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
