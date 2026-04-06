using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.WorkCenter;

namespace MaintenanceManagement.UnitTests.Validators.WorkCenter
{
    public sealed class CreateWorkCenterCommandValidatorTests
    {
        private readonly Mock<IWorkCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateWorkCenterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object);

        private void SetupAlreadyExists(string code, bool exists = false)
        {
            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync(code)).ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateWorkCenterCommand
            {
                WorkCenterCode = "WC001",
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };
            SetupAlreadyExists("WC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateWorkCenterCommand
            {
                WorkCenterCode = code,
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new CreateWorkCenterCommand
            {
                WorkCenterCode = "WC001",
                WorkCenterName = name,
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateWorkCenterCommand
            {
                WorkCenterCode = "WC001",
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };
            SetupAlreadyExists("WC001", exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
