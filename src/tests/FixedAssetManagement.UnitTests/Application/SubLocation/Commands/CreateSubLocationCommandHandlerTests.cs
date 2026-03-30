using AutoMapper;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Commands
{
    public sealed class CreateSubLocationCommandHandlerTests
    {
        private readonly Mock<ISubLocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateSubLocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = SubLocationBuilders.ValidEntity(newId);
            var dto = SubLocationBuilders.ValidDto(newId);

            _mockQueryRepo
                .Setup(r => r.IsParentLocationActiveAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.GetBySubLocationNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.SubLocation?)null);

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SubLocation>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SubLocation>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<SubLocationDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSubLocationDto()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SubLocation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = SubLocationBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
