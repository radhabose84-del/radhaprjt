using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Location.Commands
{
    public sealed class DeleteLocationCommandHandlerTests
    {
        private readonly Mock<ILocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetLocationQueryRepository> _mockAssetLocationQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockAssetLocationQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = LocationBuilders.ValidEntity(id);
            var dto = LocationBuilders.ValidDto(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockAssetLocationQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(1, int.MaxValue, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetMaster.AssetLocation>(), 0));

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.Location>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<FAM.Domain.Entities.Location>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<LocationDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsLocationDto()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.Location>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = LocationBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
