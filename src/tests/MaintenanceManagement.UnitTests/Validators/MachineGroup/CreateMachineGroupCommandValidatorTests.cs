using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MachineGroup;

namespace MaintenanceManagement.UnitTests.Validators.MachineGroup
{
    public sealed class CreateMachineGroupCommandValidatorTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMachineGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string name, bool exists = false)
        {
            _mockQueryRepo.Setup(r => r.GetByMachineGroupnameAsync(name)).ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMachineGroupCommand { GroupName = "GroupA", UnitId = 1, DepartmentId = 1, Manufacturer = 1 };
            SetupAlreadyExists("GroupA");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyGroupName_FailsValidation(string? name)
        {
            var command = new CreateMachineGroupCommand { GroupName = name, UnitId = 1, DepartmentId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateGroupName_FailsValidation()
        {
            var command = new CreateMachineGroupCommand { GroupName = "GroupA", UnitId = 1, DepartmentId = 1, Manufacturer = 1 };
            SetupAlreadyExists("GroupA", exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
