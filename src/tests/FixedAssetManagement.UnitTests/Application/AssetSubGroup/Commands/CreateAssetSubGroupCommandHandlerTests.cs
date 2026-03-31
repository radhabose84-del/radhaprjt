using AutoMapper;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubGroup.Commands
{
    public sealed class CreateAssetSubGroupCommandHandlerTests
    {
        private readonly Mock<IAssetSubGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateAssetSubGroupCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateAssetSubGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLogger.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = AssetSubGroupBuilders.ValidEntity(newId);

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetSubGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup.AssetSubGroupDto>(It.IsAny<object>()))
                .Returns(AssetSubGroupBuilders.ValidDto(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetSubGroup>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(1);
            var command = AssetSubGroupBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = AssetSubGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetSubGroup>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetSubGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
