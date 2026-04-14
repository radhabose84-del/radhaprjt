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
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateItemCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateItemCategoryCommand ValidCommand(int id = 1) =>
            new() { Id = id, ItemCategoryName = "Updated Category", ItemGroupId = 1, IsActive = 1, ModuleIds = new List<int> { 1 } };

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory { ItemCategoryName = "Updated Category" });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(3, It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()), Times.Once);
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

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .ReturnsAsync(0);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMapperOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMapper.Verify(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesMappedEntityToRepo()
        {
            var mappedEntity = new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemCategoryName = "Captured Update",
                ItemGroupId = 42
            };

            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>())).Returns(mappedEntity);

            InventoryManagement.Domain.Entities.Item.ItemCategory? capturedEntity = null;
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .Callback<int, InventoryManagement.Domain.Entities.Item.ItemCategory, List<int>>((_, e, _) => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.ItemCategoryName.Should().Be("Captured Update");
            capturedEntity.ItemGroupId.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesExactModuleIdsToRepo()
        {
            SetupHappyPath();
            var command = new UpdateItemCategoryCommand
            {
                Id = 5,
                ItemCategoryName = "Test",
                ItemGroupId = 1,
                IsActive = 1,
                ModuleIds = new List<int> { 100, 200 }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(
                5,
                It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(),
                It.Is<List<int>>(ids => ids.Count == 2 && ids[0] == 100 && ids[1] == 200)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWithLinkedItems_ThrowsWithCorrectMessage()
        {
            _mockQueryRepo.Setup(r => r.IsLinkedWithActiveItemsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var command = new UpdateItemCategoryCommand
            {
                Id = 1,
                ItemCategoryName = "Test",
                ItemGroupId = 1,
                IsActive = 0,
                ModuleIds = new List<int> { 1 }
            };

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked with other records*You cannot inactivate*");
        }

        [Fact]
        public async Task Handle_InactivateWithLinkedItems_DoesNotCallUpdateOrPublishAudit()
        {
            _mockQueryRepo.Setup(r => r.IsLinkedWithActiveItemsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var command = new UpdateItemCategoryCommand
            {
                Id = 1,
                ItemCategoryName = "Test",
                ItemGroupId = 1,
                IsActive = 0,
                ModuleIds = new List<int> { 1 }
            };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockCommandRepo.Verify(r => r.UpdateAsync(
                It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()),
                Times.Never);

            _mockMediator.Verify(m => m.Publish(
                It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RepoThrows_PropagatesException()
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<UpdateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory());

            _mockCommandRepo.Setup(r => r.UpdateAsync(
                It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .ThrowsAsync(new InvalidOperationException("Update failed"));

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Update failed");
        }
    }
}
