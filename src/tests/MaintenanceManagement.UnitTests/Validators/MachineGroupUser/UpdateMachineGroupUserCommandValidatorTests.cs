using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MachineGroupUser;

namespace MaintenanceManagement.UnitTests.Validators.MachineGroupUser
{
    public sealed class UpdateMachineGroupUserCommandValidatorTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMachineGroupUserCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllMocks(int id = 1, int machineGroupId = 1, int departmentId = 1, int userId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(machineGroupId, departmentId, userId, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMachineGroupUserCommand { Id = 1, MachineGroupId = 1, DepartmentId = 1, UserId = 1 };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new UpdateMachineGroupUserCommand { Id = 0, MachineGroupId = 1, DepartmentId = 1, UserId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DuplicateMapping_FailsValidation()
        {
            var command = new UpdateMachineGroupUserCommand { Id = 1, MachineGroupId = 1, DepartmentId = 1, UserId = 1 };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, 1, 1, 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
