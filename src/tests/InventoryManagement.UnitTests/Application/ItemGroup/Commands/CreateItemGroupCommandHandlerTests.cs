using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemGroup.Commands
{
    public sealed class CreateItemGroupCommandHandlerTests
    {
        private readonly Mock<IItemGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateItemGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = ItemGroupBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(It.IsAny<CreateItemGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);

            var result = await CreateSut().Handle(ItemGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ItemGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ItemGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ItemGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsExceptionRules()
        {
            var entity = ItemGroupBuilders.ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(It.IsAny<CreateItemGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemGroup>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ItemGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
