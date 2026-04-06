using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MachineMaster;

namespace MaintenanceManagement.UnitTests.Validators.MachineMaster
{
    public sealed class UpdateMachineMasterCommandValidatorTests
    {
        private readonly Mock<IMachineMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMachineMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllMocks(int id = 1, string code = "M001", string name = "Machine A", int machineGroupId = 1, int unitId = 1)
        {
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(name, machineGroupId, id)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.IsCodeDuplicateAsync(code, unitId, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster.MachineMasterDto { Id = id });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMachineMasterCommand { Id = 1, MachineCode = "M001", MachineName = "Machine A", MachineGroupId = 1, UnitId = 1 };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMachineCode_FailsValidation(string? code)
        {
            var command = new UpdateMachineMasterCommand { Id = 1, MachineCode = code, MachineName = "Machine A", MachineGroupId = 1, UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateMachineMasterCommand { Id = 99, MachineCode = "M001", MachineName = "Machine A", MachineGroupId = 1, UnitId = 1 };
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync("Machine A", 1, 99)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.IsCodeDuplicateAsync("M001", 1, 99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster.MachineMasterDto?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
