using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Commands
{
    public sealed class UpdateItemCategoryCommandHandlerTests
    {
        private readonly Mock<IItemCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateItemCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateItemCategoryCommand ValidCommand(int id = 1) =>
            new() { Id = id, ItemCategoryName = "Updated Category", ItemGroupId = 1, IsActive = 1 };

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory { ItemCategoryName = "Updated Category" });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(id: 3), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(3, It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory());

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
                .ReturnsAsync(0);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
