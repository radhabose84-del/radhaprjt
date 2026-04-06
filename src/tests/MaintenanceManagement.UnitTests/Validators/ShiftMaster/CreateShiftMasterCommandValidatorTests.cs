using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.ShiftMaster;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMaster
{
    public sealed class CreateShiftMasterCommandValidatorTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQuery = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateShiftMasterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQuery.Object);

        private void SetupAllMocks(string shiftName = "Morning", string shiftCode = "SH001")
        {
            _mockQuery.Setup(r => r.AlreadyExistsAsync(shiftName, null)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync(shiftCode, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateShiftMasterCommand
            {
                ShiftCode = "SH001",
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyShiftCode_FailsValidation(string? code)
        {
            var command = new CreateShiftMasterCommand
            {
                ShiftCode = code,
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyShiftName_FailsValidation(string? name)
        {
            var command = new CreateShiftMasterCommand
            {
                ShiftCode = "SH001",
                ShiftName = name,
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateShiftName_FailsValidation()
        {
            var command = new CreateShiftMasterCommand
            {
                ShiftCode = "SH001",
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };
            _mockQuery.Setup(r => r.AlreadyExistsAsync("Morning", null)).ReturnsAsync(true);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync("SH001", null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateShiftCode_FailsValidation()
        {
            var command = new CreateShiftMasterCommand
            {
                ShiftCode = "SH001",
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };
            _mockQuery.Setup(r => r.AlreadyExistsAsync("Morning", null)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync("SH001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
