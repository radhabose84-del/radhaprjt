#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Presentation.Validation.MiscTypeMaster;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandValidator CreateValidator()
            => new CreateMiscTypeMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        private void SetupCodeNotExists(string code = "MISC001")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
        }

        private void SetupCodeAlreadyExists(string code)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(true);
        }

        private void SetupAnyCodeNotExists()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupCodeNotExists("MISC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── MiscTypeCode Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MiscTypeCode_Empty_FailsValidation(string code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("Misc Type Code is required.");
        }

        [Fact]
        public async Task MiscTypeCode_TooLong_FailsValidation()
        {
            var longCode = new string('A', 21);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("Misc Type Code cannot exceed 20 characters.");
        }

        [Theory]
        [InlineData("MISC-01")]
        [InlineData("MISC 01")]
        [InlineData("MISC@01")]
        [InlineData("MISC.01")]
        public async Task MiscTypeCode_NotAlphanumeric_FailsValidation(string code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("Misc Type Code must be alphanumeric only.");
        }

        [Fact]
        public async Task MiscTypeCode_AlreadyExists_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "MISC001");
            SetupCodeAlreadyExists("MISC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("Misc Type Code already exists.");
        }

        [Fact]
        public async Task MiscTypeCode_Unique_PassesExistenceCheck()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "MISC999");
            SetupCodeNotExists("MISC999");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task MiscTypeCode_MaxLength20_PassesValidation()
        {
            var maxCode = new string('A', 20);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: maxCode);
            SetupCodeNotExists(maxCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        // ── Description Rules ─────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string description)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: description);
            SetupCodeNotExists("MISC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupCodeNotExists("MISC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description cannot exceed 250 characters.");
        }

        [Fact]
        public async Task Description_MaxLength250_PassesValidation()
        {
            var maxDesc = new string('X', 250);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: maxDesc);
            SetupCodeNotExists("MISC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
