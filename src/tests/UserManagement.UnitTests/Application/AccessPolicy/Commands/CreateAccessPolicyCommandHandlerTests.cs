using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Commands
{
    public sealed class CreateAccessPolicyCommandHandlerTests
    {
        private readonly Mock<IAccessPolicyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAccessPolicyQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IMediator>                      _mockMediator    = new(MockBehavior.Loose);
        private readonly Mock<IMapper>                        _mockMapper      = new(MockBehavior.Strict);

        private CreateAccessPolicyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateAccessPolicyCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.AccessPolicy>(command))
                .Returns(AccessPolicyBuilders.ValidEntity(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.AccessPolicy>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = AccessPolicyBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = AccessPolicyBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = AccessPolicyBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.AccessPolicy>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = AccessPolicyBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode   == "ACCESSPOLICY_CREATE" &&
                        e.Module       == "AccessPolicy"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SuccessMessageSet()
        {
            var command = AccessPolicyBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Be("Access Policy created successfully.");
        }
    }
}
