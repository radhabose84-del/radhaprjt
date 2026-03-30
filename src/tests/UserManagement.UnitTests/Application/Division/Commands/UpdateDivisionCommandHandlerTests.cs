using AutoMapper;
using MediatR;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Division.Commands
{
    public sealed class UpdateDivisionCommandHandlerTests
    {
        private readonly Mock<IDivisionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDivisionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UpdateDivisionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateDivisionCommand command, UserManagement.Domain.Entities.Division mappedEntity)
        {
            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.Division?)null);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var entity = DivisionBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var entity = DivisionBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var entity = DivisionBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "Division"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var existingDivision = DivisionBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name, command.Id))
                .ReturnsAsync(existingDivision);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var entity = DivisionBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.Division?)null);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not updated*");
        }
    }
}
