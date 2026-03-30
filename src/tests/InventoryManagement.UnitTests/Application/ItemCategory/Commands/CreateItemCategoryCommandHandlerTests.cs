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
            new() { ItemCategoryName = name, ItemGroupId = groupId };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<CreateItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory { ItemCategoryName = "Test Category" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
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
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()), Times.Once);
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

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
                .ReturnsAsync(0);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*ItemCategory Creation Failed*");
        }
    }
}
