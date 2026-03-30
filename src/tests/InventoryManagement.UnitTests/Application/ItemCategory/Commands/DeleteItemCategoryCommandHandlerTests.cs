using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Commands
{
    public sealed class DeleteItemCategoryCommandHandlerTests
    {
        private readonly Mock<IItemCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteItemCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<DeleteItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory { ItemCategoryName = "Cat" });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(new DeleteItemCategoryCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteItemCategoryCommand { Id = 5 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(5, It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteItemCategoryCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(
                It.IsAny<DeleteItemCategoryCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.Item.ItemCategory());

            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemCategory>()))
                .ReturnsAsync(0);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act = async () => await CreateSut().Handle(new DeleteItemCategoryCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }
    }
}
