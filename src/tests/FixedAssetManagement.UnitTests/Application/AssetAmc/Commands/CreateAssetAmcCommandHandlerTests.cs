using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Commands
{
    public sealed class CreateAssetAmcCommandHandlerTests
    {
        private readonly Mock<IAssetAmcCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateAssetAmcCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = AssetAmcBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(newId);

            _mockMapper
                .Setup(m => m.Map<AssetAmcDto>(It.IsAny<object>()))
                .Returns(AssetAmcBuilders.ValidDto(newId));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var entity = AssetAmcBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(0);

            _mockMapper
                .Setup(m => m.Map<AssetAmcDto>(It.IsAny<object>()))
                .Returns(AssetAmcBuilders.ValidDto());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = AssetAmcBuilders.ValidCreateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
