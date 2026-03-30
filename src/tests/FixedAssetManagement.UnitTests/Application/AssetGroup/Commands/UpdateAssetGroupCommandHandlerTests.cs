using AutoMapper;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetGroup.Commands
{
    public sealed class UpdateAssetGroupCommandHandlerTests
    {
        private readonly Mock<IAssetGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateAssetGroupCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetGroupBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.IsAssetGroupLinkedAsync(id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<int>(), id, It.IsAny<decimal>()))
                .ReturnsAsync((false, false));

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<FAM.Domain.Entities.AssetGroup>()))
                .ReturnsAsync(id);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(1);
            var command = AssetGroupBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = AssetGroupBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetGroup>()), Times.Once);
        }
    }
}
