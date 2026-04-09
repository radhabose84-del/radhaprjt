using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.CostCenter;

namespace MaintenanceManagement.UnitTests.Validators.CostCenter
{
    public sealed class CreateCostCenterCommandValidatorTests
    {
        private readonly Mock<ICostCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateCostCenterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object);

        private void SetupAlreadyExists(string code, string name, int unitId, bool exists = false)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeOrNameAndUnitAsync(code, name, unitId))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateCostCenterCommand
            {
                CostCenterCode = "CC001",
                CostCenterName = "Cost Center 1",
                UnitId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTime.UtcNow,
                ResponsiblePerson = "John Doe"
            };
            SetupAlreadyExists(command.CostCenterCode!, command.CostCenterName!, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCostCenterCode_FailsValidation(string? code)
        {
            var command = new CreateCostCenterCommand
            {
                CostCenterCode = code,
                CostCenterName = "Cost Center 1",
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateCostCenterCommand
            {
                CostCenterCode = "CC001",
                CostCenterName = "Cost Center 1",
                UnitId = 1,
                DepartmentId = 1
            };
            SetupAlreadyExists(command.CostCenterCode!, command.CostCenterName!, command.UnitId, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
