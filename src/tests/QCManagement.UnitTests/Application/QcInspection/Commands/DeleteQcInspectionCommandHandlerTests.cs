using Contracts.Common;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.DeleteQcInspection;

namespace QCManagement.UnitTests.Application.QcInspection.Commands
{
    public class DeleteQcInspectionCommandHandlerTests
    {
        private readonly Mock<IQcInspectionCommandRepository> _cmd = new(MockBehavior.Strict);
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);

        private DeleteQcInspectionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _mediator.Object);

        [Fact]
        public async Task Handle_Existing_ReturnsTrue()
        {
            _cmd.Setup(c => c.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteQcInspectionCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistent_ThrowsExceptionRules()
        {
            _cmd.Setup(c => c.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new DeleteQcInspectionCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistent_DoesNotPublishAudit()
        {
            _cmd.Setup(c => c.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            try { await CreateSut().Handle(new DeleteQcInspectionCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
