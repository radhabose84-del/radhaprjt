using AutoMapper;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.SubLocation.Command.UpdateSubLocation;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Commands
{
    public sealed class UpdateSubLocationCommandHandlerTests
    {
        private readonly Mock<ISubLocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateSubLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = SubLocationBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.IsSubLocationLinkedAsync(id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.IsParentLocationActiveAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SubLocation>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.SubLocation>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.SubLocation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
