using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Department.Commands
{
    public sealed class CreateDepartmentCommandHandlerTests
    {
        private readonly Mock<IDepartmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateDepartmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateDepartmentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(CreateDepartmentCommand command, UserManagement.Domain.Entities.Department createdEntity, DepartmentDto dto)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.DeptName!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Department>(command))
                .Returns(createdEntity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Department>()))
                .ReturnsAsync(createdEntity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<DepartmentDto>(createdEntity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDepartmentDto()
        {
            var command = DepartmentBuilders.ValidCreateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            var dto = DepartmentBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.DeptName.Should().Be("Test Department");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = DepartmentBuilders.ValidCreateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            var dto = DepartmentBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Department>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DepartmentBuilders.ValidCreateCommand();
            var entity = DepartmentBuilders.ValidEntity();
            var dto = DepartmentBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateDeptName_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidCreateCommand();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.DeptName!))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsNull_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidCreateCommand();
            var entity = DepartmentBuilders.ValidEntity();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.DeptName!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Department>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Department>()))
                .ReturnsAsync((UserManagement.Domain.Entities.Department?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not created*");
        }
    }
}
