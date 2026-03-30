using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            var entity = MiscMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            var entity = MiscMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            var entity = MiscMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()))
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
