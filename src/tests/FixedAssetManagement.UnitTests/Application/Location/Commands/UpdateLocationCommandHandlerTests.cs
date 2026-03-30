using AutoMapper;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Location.Commands
{
    public sealed class UpdateLocationCommandHandlerTests
    {
        private readonly Mock<ILocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.IsLinkedWithSubLocationsAsync(id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<int>(), id))
                .ReturnsAsync((false, false));

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.Location>(It.IsAny<object>()))
                .Returns(LocationBuilders.ValidEntity(id));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.Location>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.Location>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
