using AutoMapper;
using Contracts.Common;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationMaster.Commands
{
    public sealed class UpdateItemSpecificationMasterCommandHandlerTests
    {
        private readonly Mock<IItemSpecificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateItemSpecificationMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            var entity = ItemSpecificationMasterBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<DomainEntities.ItemSpecificationMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<DomainEntities.ItemSpecificationMaster>()))
                .ReturnsAsync(updatedId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<DomainEntities.ItemSpecificationMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "ITEMSPECIFICATIONMASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWhenLinked_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(id: 1, isActive: 0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }

        [Fact]
        public async Task Handle_InactivateWhenNotLinked_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidUpdateCommand(id: 1, isActive: 0),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
