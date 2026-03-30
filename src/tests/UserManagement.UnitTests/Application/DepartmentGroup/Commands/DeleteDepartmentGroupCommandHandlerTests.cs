using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Commands
{
    public sealed class DeleteDepartmentGroupCommandHandlerTests
    {
        private readonly Mock<IDepartmentGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteDepartmentGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UserManagement.Domain.Entities.DepartmentGroup ValidEntity(int id = 1) =>
            new() { Id = id, DepartmentGroupCode = "DG001", DepartmentGroupName = "Test Dept Group" };

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            var command = new DeleteDepartmentGroupCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var command = new DeleteDepartmentGroupCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var command = new DeleteDepartmentGroupCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsValidationException()
        {
            var command = new DeleteDepartmentGroupCommand { Id = 99 };
            var entity = ValidEntity(99);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Failed to delete*");
        }
    }
}
