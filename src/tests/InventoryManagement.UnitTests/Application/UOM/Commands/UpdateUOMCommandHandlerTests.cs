using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOM.Commands
{
    public sealed class UpdateUOMCommandHandlerTests
    {
        private readonly Mock<IUOMCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUOMCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(bool updateResult = true)
        {
            var entity = UOMBuilders.ValidEntity(1);
            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((InventoryManagement.Domain.Entities.UOM?)null);
            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<InventoryManagement.Domain.Entities.UOM>()))
                .ReturnsAsync(updateResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                UOMBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFailed_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(
                UOMBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
