using AutoMapper;
using Contracts.Common;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Commands
{
    public sealed class UpdateItemSpecificationValueCommandHandlerTests
    {
        private readonly Mock<IItemSpecificationValueCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateItemSpecificationValueCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            var entity = ItemSpecificationValueBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<DomainEntities.ItemSpecificationValue>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<DomainEntities.ItemSpecificationValue>()))
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
                ItemSpecificationValueBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<DomainEntities.ItemSpecificationValue>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "ITEMSPECIFICATIONVALUE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWhenLinked_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationValueLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(id: 1, isActive: 0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }

        [Fact]
        public async Task Handle_InactivateWhenNotLinked_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationValueLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidUpdateCommand(id: 1, isActive: 0),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
