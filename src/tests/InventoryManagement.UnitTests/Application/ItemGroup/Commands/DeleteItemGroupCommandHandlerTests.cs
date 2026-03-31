using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemGroup.Commands
{
    public sealed class DeleteItemGroupCommandHandlerTests
    {
        private readonly Mock<IItemGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteItemGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            var entity = ItemGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(It.IsAny<DeleteItemGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(result: 1);

            var result = await CreateSut().Handle(ItemGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ItemGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ItemGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            var entity = ItemGroupBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(It.IsAny<DeleteItemGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ItemGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
