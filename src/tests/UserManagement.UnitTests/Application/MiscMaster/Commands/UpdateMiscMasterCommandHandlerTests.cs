using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(UpdateMiscMasterCommand command, bool updateResult = true)
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster
            {
                Id = command.Id,
                Code = command.Code,
                Description = command.Description
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(updateResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MC001", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MC001", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<UserManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MC001", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MC001", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*updation failed*");
        }
    }
}
