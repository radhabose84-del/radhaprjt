using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.SubLocation.Command.DeleteSubLocation;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Commands
{
    public sealed class DeleteSubLocationCommandHandlerTests
    {
        private readonly Mock<ISubLocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILocationQueryRepository> _mockLocationQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetLocationQueryRepository> _mockAssetLocationQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteSubLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockAssetLocationQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockLocationQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = SubLocationBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.IsSubLocationLinkedAsync(id))
                .ReturnsAsync(false);

            _mockAssetLocationQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(1, 1, id.ToString()))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetMaster.AssetLocation>(), 0));

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SubLocation>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<FAM.Domain.Entities.SubLocation>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.SubLocation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
