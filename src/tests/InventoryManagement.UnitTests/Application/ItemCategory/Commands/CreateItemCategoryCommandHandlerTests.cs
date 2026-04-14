using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Commands
{
    public sealed class CreateItemCategoryCommandHandlerTests
    {
        private readonly Mock<IItemCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateItemCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateItemCategoryCommand ValidCommand(string name = "Test Category", int groupId = 1) =>
            new() { ItemCategoryName = name, ItemGroupId = groupId, ModuleIds = new List<int> { 1 } };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory { ItemCategoryName = "Test Category" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 5);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory());

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .ReturnsAsync(0);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*ItemCategory Creation Failed*");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMapperOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMapper.Verify(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesMappedEntityToRepo()
        {
            var mappedEntity = new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemCategoryName = "Captured Category",
                ItemGroupId = 7
            };

            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>())).Returns(mappedEntity);

            InventoryManagement.Domain.Entities.Item.ItemCategory? capturedEntity = null;
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .Callback<InventoryManagement.Domain.Entities.Item.ItemCategory, List<int>>((e, _) => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.ItemCategoryName.Should().Be("Captured Category");
            capturedEntity.ItemGroupId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesExactModuleIdsToRepo()
        {
            SetupHappyPath();
            var command = new CreateItemCategoryCommand
            {
                ItemCategoryName = "Test",
                ItemGroupId = 1,
                ModuleIds = new List<int> { 10, 20, 30 }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(),
                It.Is<List<int>>(ids => ids.Count == 3 && ids[0] == 10 && ids[1] == 20 && ids[2] == 30)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditWithCorrectModuleAndAction()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(name: "Audit Test"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.Module == "itemCategory" &&
                    e.Details != null && e.Details.Contains("Item Category")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepoThrows_PropagatesException()
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory());

            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>(), It.IsAny<List<int>>()))
                .ThrowsAsync(new InvalidOperationException("DB connection lost"));

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("DB connection lost");
        }
    }
}
