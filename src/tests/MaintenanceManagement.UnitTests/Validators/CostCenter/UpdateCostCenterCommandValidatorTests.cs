using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.CostCenter;

namespace MaintenanceManagement.UnitTests.Validators.CostCenter
{
    public sealed class UpdateCostCenterCommandValidatorTests
    {
        private readonly Mock<ICostCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateCostCenterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object, _mockQueryRepo.Object);

        private void SetupAllMocks(int id = 1, string name = "Cost Center 1", int unitId = 1)
        {
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(name, id, unitId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.CostCenter { Id = id });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateCostCenterCommand
            {
                Id = 1,
                CostCenterName = "Cost Center 1",
                UnitId = 1,
                DepartmentId = 1
            };
            SetupAllMocks(command.Id, command.CostCenterName!, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new UpdateCostCenterCommand { Id = 1, CostCenterName = name, UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateCostCenterCommand { Id = 99, CostCenterName = "Cost Center X", UnitId = 1 };
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(command.CostCenterName!, command.Id, command.UnitId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MaintenanceManagement.Domain.Entities.CostCenter?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
