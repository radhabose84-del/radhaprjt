using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.UOM.Commands
{
    public sealed class UpdateUOMCommandHandlerTests
    {
        private readonly Mock<IUOMCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUOMCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = FAMUOMBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.UOM?)null);

            _mockQueryRepo
                .Setup(r => r.IsUomLinkedAsync(id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((false, false));

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.UOM>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.UOM>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactiveWithLinkedRecords_ThrowsValidationException()
        {
            var entity = FAMUOMBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.UOM?)null);

            _mockQueryRepo
                .Setup(r => r.IsUomLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);

            var sut = CreateSut();
            var command = FAMUOMBuilders.ValidUpdateCommand(isActive: 0);

            await Assert.ThrowsAsync<FluentValidation.ValidationException>(
                () => sut.Handle(command, CancellationToken.None));
        }
    }
}
