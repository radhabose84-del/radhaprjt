using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOM.Commands
{
    public sealed class DeleteUOMCommandHandlerTests
    {
        private readonly Mock<IUOMCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteUOMCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool deleteResult = true)
        {
            var entity = UOMBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOM>()))
                .ReturnsAsync(deleteResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                UOMBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFailed_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(
                UOMBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(UOMBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOM>()), Times.Once);
        }
    }
}
