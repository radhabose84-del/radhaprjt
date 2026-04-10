using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MachineGroupUser;

namespace MaintenanceManagement.UnitTests.Validators.MachineGroupUser
{
    public sealed class CreateMachineGroupUserCommandValidatorTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMachineGroupUserCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAlreadyExists(int machineGroupId, int departmentId, int userId, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(machineGroupId, departmentId, userId, null))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMachineGroupUserCommand { MachineGroupId = 1, DepartmentId = 1, UserId = 1 };
            SetupAlreadyExists(1, 1, 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroMachineGroupId_FailsValidation()
        {
            var command = new CreateMachineGroupUserCommand { MachineGroupId = 0, DepartmentId = 1, UserId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateMapping_FailsValidation()
        {
            var command = new CreateMachineGroupUserCommand { MachineGroupId = 1, DepartmentId = 1, UserId = 1 };
            SetupAlreadyExists(1, 1, 1, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
