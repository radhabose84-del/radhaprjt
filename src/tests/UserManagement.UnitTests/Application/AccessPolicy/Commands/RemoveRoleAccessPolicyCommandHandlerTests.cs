using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.AccessPolicy.Commands
{
    public sealed class RemoveRoleAccessPolicyCommandHandlerTests
    {
        private readonly Mock<IAccessPolicyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator>                      _mockMediator    = new(MockBehavior.Loose);

        private RemoveRoleAccessPolicyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.RemoveRoleValueAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new RemoveRoleAccessPolicyCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.RemoveRoleValueAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new RemoveRoleAccessPolicyCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.ActionCode   == "ROLEACCESSPOLICY_REMOVE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.RemoveRoleValueAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new RemoveRoleAccessPolicyCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.RemoveRoleValueAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            try { await CreateSut().Handle(new RemoveRoleAccessPolicyCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
