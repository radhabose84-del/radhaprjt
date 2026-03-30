using AutoMapper;
using MediatR;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.HSNMaster.Commands
{
    public sealed class DeleteHSNMasterCommandHandlerTests
    {
        private readonly Mock<IHSNMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteHSNMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                HSNMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallDelete()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            await CreateSut().Handle(HSNMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.HSNMaster>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingRecord_ReturnsSuccess()
        {
            var entity = HSNMasterBuilders.ValidEntity(1);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(false);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.HSNMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<InventoryManagement.Domain.Entities.HSNMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                HSNMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuccessfulDelete_PublishesAuditEvent()
        {
            var entity = HSNMasterBuilders.ValidEntity(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.HSNMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<InventoryManagement.Domain.Entities.HSNMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(HSNMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "SoftDelete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
