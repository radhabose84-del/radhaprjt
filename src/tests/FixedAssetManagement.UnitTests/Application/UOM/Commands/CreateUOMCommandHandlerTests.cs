using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.UOM.Commands
{
    public sealed class CreateUOMCommandHandlerTests
    {
        private readonly Mock<IUOMCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateUOMCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = FAMUOMBuilders.ValidEntity(id);
            var dto = FAMUOMBuilders.ValidDto(id);

            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.UOM?)null);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.UOM>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UOMDto>(It.IsAny<FAM.Domain.Entities.UOM>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.UOM>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = FAMUOMBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingUOMName_ThrowsValidationException()
        {
            var entity = FAMUOMBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByUOMNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.UOM>(It.IsAny<object>()))
                .Returns(entity);

            var sut = CreateSut();
            var command = FAMUOMBuilders.ValidCreateCommand();

            await Assert.ThrowsAsync<FluentValidation.ValidationException>(
                () => sut.Handle(command, CancellationToken.None));
        }
    }
}
