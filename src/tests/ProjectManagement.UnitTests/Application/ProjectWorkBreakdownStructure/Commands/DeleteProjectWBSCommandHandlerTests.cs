using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.DeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Commands
{
    public sealed class DeleteProjectWBSCommandHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeleteProjectWorkBreakdownStructureCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private DeleteProjectWorkBreakdownStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_DeleteSucceeds_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                ProjectWorkBreakdownStructureBuilders.ValidDeleteCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*deletion failed*");
        }

        [Fact]
        public async Task Handle_DeleteSucceeds_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteSucceeds_CallsDeleteOnce()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
