using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_NoExistingDuplicate_ReturnsSuccess()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync((ProjectManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DuplicateExists_ReturnsFailure()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            var existing = MiscTypeMasterBuilders.ValidEntity(id: 99);

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync(existing);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync((ProjectManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync((ProjectManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
