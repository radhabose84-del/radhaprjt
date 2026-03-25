using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Commands
{
    public sealed class UpdateAssetAmcCommandHandlerTests
    {
        private readonly Mock<IAssetAmcCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateAssetAmcCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetAmcBuilders.ValidEntity(id);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsValidationException()
        {
            var entity = AssetAmcBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(0);

            var command = AssetAmcBuilders.ValidUpdateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
