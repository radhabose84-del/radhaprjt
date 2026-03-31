using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Commands
{
    public sealed class DeleteAssetAmcCommandHandlerTests
    {
        private readonly Mock<IAssetAmcCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetAmcQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAssetAmcCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetAmcBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = AssetAmcBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.AssetMaster.AssetAmc?)null);

            var command = AssetAmcBuilders.ValidDeleteCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteReturnsMinusOne_ThrowsValidationException()
        {
            var entity = AssetAmcBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetAmc>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetAmc>()))
                .ReturnsAsync(-1);

            var command = AssetAmcBuilders.ValidDeleteCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
