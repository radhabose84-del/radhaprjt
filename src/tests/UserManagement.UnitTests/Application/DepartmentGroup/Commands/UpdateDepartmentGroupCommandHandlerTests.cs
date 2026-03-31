using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Commands
{
    public sealed class UpdateDepartmentGroupCommandHandlerTests
    {
        private readonly Mock<IDepartmentGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateDepartmentGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object);

        private static UpdateDepartmentGroupCommand ValidCommand(int id = 1) =>
            new()
            {
                Id = id,
                DepartmentGroupCode = "DG001",
                DepartmentGroupName = "Updated Dept Group",
                IsActive = Status.Active
            };

        private static UserManagement.Domain.Entities.DepartmentGroup ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                DepartmentGroupCode = "DG001",
                DepartmentGroupName = "Original Dept Group"
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsOne()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = ValidCommand(99);

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.DepartmentGroup?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_InactiveWithLinkedDepartments_ThrowsValidationException()
        {
            var command = new UpdateDepartmentGroupCommand
            {
                Id = 1,
                DepartmentGroupCode = "DG001",
                DepartmentGroupName = "Test",
                IsActive = Status.Inactive
            };
            var entity = ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.IsLinkedWithDepartmentsAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked*");
        }
    }
}
