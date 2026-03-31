using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(UpdateMiscTypeMasterCommand command, bool updateResult = true)
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster
            {
                Id = command.Id,
                MiscTypeCode = command.MiscTypeCode,
                Description = command.Description
            };

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(updateResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MISC01", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MISC01", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MISC01", Description = "Updated", IsActive = 1 };
            SetupHappyPath(command, updateResult: true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ReturnsFailureResponse()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "DUPCODE", Description = "Updated", IsActive = 1 };
            var existing = new UserManagement.Domain.Entities.MiscTypeMaster { Id = 99, MiscTypeCode = "DUPCODE" };

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync("DUPCODE", 1))
                .ReturnsAsync(existing);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }
    }
}
