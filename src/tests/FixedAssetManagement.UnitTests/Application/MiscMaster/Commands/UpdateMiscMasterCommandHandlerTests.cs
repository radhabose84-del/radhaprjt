using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            var entity = MiscMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.IsMiscMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync((FAM.Domain.Entities.MiscMaster?)null);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.MiscMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InactiveWithLinkedRecords_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.IsMiscMasterLinkedAsync(1))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                MiscMasterBuilders.ValidUpdateCommand(isActive: 0), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsValidationException()
        {
            var existing = MiscMasterBuilders.ValidEntity(99);

            _mockQueryRepo
                .Setup(r => r.IsMiscMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                MiscMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
