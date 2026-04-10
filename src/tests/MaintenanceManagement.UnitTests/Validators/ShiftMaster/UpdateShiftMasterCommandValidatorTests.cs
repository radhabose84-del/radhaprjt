using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.ShiftMaster;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMaster
{
    public sealed class UpdateShiftMasterCommandValidatorTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateShiftMasterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQuery.Object);

        private void SetupAllMocks(int id = 1, string shiftName = "Morning", string shiftCode = "SH001")
        {
            _mockQuery.Setup(r => r.AlreadyExistsAsync(shiftName, id)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync(shiftCode, id)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateShiftMasterCommand
            {
                Id = 1,
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
        public async Task Validate_EmptyShiftName_FailsValidation(string? name)
        {
            var command = new UpdateShiftMasterCommand
            {
                Id = 1,
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
            var command = new UpdateShiftMasterCommand
            {
                Id = 1,
                ShiftCode = "SH001",
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };
            _mockQuery.Setup(r => r.AlreadyExistsAsync("Morning", 1)).ReturnsAsync(true);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync("SH001", 1)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateShiftMasterCommand
            {
                Id = 99,
                ShiftCode = "SH001",
                ShiftName = "Morning",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };
            _mockQuery.Setup(r => r.AlreadyExistsAsync("Morning", 99)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.AlreadyExistsShiftCodeAsync("SH001", 99)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
