using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.Department.Commands
{
    public sealed class UpdateDepartmentCommandHandlerTests
    {
        private readonly Mock<IDepartmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateDepartmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UpdateDepartmentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object);

        private void SetupHappyPath(UpdateDepartmentCommand command, UserManagement.Domain.Entities.Department mappedEntity)
        {
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Department>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Department>()))
                .ReturnsAsync(1);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.DeptName!, command.Id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(DepartmentBuilders.ValidEntity());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Department>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();
            var entity = DepartmentBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Department>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Department>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Failed to update*");
        }

        [Fact]
        public async Task Handle_InactivateWhileLinked_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidUpdateCommand(isActive: Status.Inactive);

            _mockQueryRepo
                .Setup(r => r.IsDepartmentLinkedAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked with other records*");
        }
    }
}
