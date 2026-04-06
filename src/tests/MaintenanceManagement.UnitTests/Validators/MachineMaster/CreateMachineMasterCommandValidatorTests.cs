using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MachineMaster;

namespace MaintenanceManagement.UnitTests.Validators.MachineMaster
{
    public sealed class CreateMachineMasterCommandValidatorTests
    {
        private readonly Mock<IMachineMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMachineMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object);

        private void SetupAllMocks(string code = "M001", string name = "Machine A", int machineGroupId = 1)
        {
            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync(code)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(name, machineGroupId, 0)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMachineMasterCommand { MachineCode = "M001", MachineName = "Machine A", MachineGroupId = 1, UnitId = 1 };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMachineCode_FailsValidation(string? code)
        {
            var command = new CreateMachineMasterCommand { MachineCode = code, MachineName = "Machine A", MachineGroupId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateMachineMasterCommand { MachineCode = "M001", MachineName = "Machine A", MachineGroupId = 1, UnitId = 1 };
            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync("M001")).ReturnsAsync(true);
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync("Machine A", 1, 0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
