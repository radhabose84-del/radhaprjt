using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UsageType.Commands
{
    public sealed class CreateUsageTypeCommandHandlerTests
    {
        private readonly Mock<IUsageTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateUsageTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = UsageTypeBuilders.ValidEntity(newId);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UsageType>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.UsageType>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                UsageTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(
                UsageTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(UsageTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.UsageType>()), Times.Once);
        }
    }
}
