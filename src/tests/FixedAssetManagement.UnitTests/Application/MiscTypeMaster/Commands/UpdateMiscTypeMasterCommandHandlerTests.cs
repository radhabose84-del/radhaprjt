using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsValidationException()
        {
            var existing = MiscTypeMasterBuilders.ValidEntity(99);

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
