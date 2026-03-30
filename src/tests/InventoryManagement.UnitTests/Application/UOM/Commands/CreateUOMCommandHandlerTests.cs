using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOM.Commands
{
    public sealed class CreateUOMCommandHandlerTests
    {
        private readonly Mock<IUOMCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateUOMCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = UOMBuilders.ValidEntity(id);
            var dto = UOMBuilders.ValidDto(id);
            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), null))
                .ReturnsAsync((InventoryManagement.Domain.Entities.UOM?)null);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.UOM>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<UOMDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                UOMBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(UOMBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.UOM>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingUOM_ReturnsFailure()
        {
            var existing = UOMBuilders.ValidEntity(2);
            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), null))
                .ReturnsAsync(existing);

            var result = await CreateSut().Handle(
                UOMBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
