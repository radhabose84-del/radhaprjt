using AutoMapper;
using MediatR;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.Division.Commands
{
    public sealed class DeleteDivisionCommandHandlerTests
    {
        private readonly Mock<IDivisionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteDivisionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(DeleteDivisionCommand command)
        {
            var mappedEntity = new UserManagement.Domain.Entities.Division
            {
                Id = 0,
                IsDeleted = IsDelete.Deleted
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = DivisionBuilders.ValidDeleteCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DivisionBuilders.ValidDeleteCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "Division"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var command = DivisionBuilders.ValidDeleteCommand();
            var mappedEntity = new UserManagement.Domain.Entities.Division
            {
                Id = 0,
                IsDeleted = IsDelete.Deleted
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not deleted*");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            var command = DivisionBuilders.ValidDeleteCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Division>()),
                Times.Once);
        }
    }
}
