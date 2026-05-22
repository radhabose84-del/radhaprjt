using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Commands
{
    public sealed class UpdateAccessPolicyCommandHandlerTests
    {
        private readonly Mock<IAccessPolicyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAccessPolicyQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IMediator>                      _mockMediator    = new(MockBehavior.Loose);
        private readonly Mock<IMapper>                        _mockMapper      = new(MockBehavior.Strict);

        private UpdateAccessPolicyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateAccessPolicyCommand command)
        {
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AccessPolicy>(command))
                .Returns(AccessPolicyBuilders.ValidEntity(command.Id));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.AccessPolicy>()))
                .ReturnsAsync(command.Id);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = AccessPolicyBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            var command = AccessPolicyBuilders.ValidUpdateCommand(id: 5);
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = AccessPolicyBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.AccessPolicy>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = AccessPolicyBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode   == "ACCESSPOLICY_UPDATE" &&
                        e.Module       == "AccessPolicy"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
