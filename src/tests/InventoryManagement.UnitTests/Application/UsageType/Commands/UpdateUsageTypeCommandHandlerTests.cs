using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UsageType.Commands
{
    public sealed class UpdateUsageTypeCommandHandlerTests
    {
        private readonly Mock<IUsageTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateUsageTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            var entity = UsageTypeBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UsageType>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<InventoryManagement.Domain.Entities.UsageType>()))
                .ReturnsAsync(result);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                UsageTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(UsageTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<InventoryManagement.Domain.Entities.UsageType>()), Times.Once);
        }
    }
}
