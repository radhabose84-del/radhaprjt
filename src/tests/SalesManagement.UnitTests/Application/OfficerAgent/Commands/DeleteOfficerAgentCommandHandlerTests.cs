using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Commands
{
    public class DeleteOfficerAgentCommandHandlerTests
    {
        private readonly Mock<IOfficerAgentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteOfficerAgentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupDelete(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupDelete(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(new DeleteOfficerAgentCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteAsync_Once()
        {
            SetupDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteOfficerAgentCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
        {
            SetupDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteOfficerAgentCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "OFFICER_AGENT_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new DeleteOfficerAgentCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteOfficerAgentCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
