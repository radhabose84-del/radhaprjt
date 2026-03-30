using AutoMapper;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Location.Commands
{
    public sealed class CreateLocationCommandHandlerTests
    {
        private readonly Mock<ILocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = LocationBuilders.ValidEntity(newId);
            var dto = LocationBuilders.ValidDto(newId);

            _mockQueryRepo
                .Setup(r => r.GetByLocationNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.Location?)null);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.Location>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.Location>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<LocationDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsLocationDto()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.Location>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
