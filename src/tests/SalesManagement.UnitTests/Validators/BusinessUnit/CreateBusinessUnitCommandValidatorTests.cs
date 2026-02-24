#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Presentation.Validation.BusinessUnit;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.BusinessUnit
{
    public class CreateBusinessUnitCommandValidatorTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateBusinessUnitCommandValidator CreateValidator()
            => new CreateBusinessUnitCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupCodeNotExists(string code = "BU001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);
        }

        private void SetupCodeAlreadyExists(string code)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(true);
        }

        /// <summary>
        /// Use when the test doesn't care about uniqueness.
        /// FluentValidation runs ALL rules regardless of earlier failures.
        /// </summary>
        private void SetupAnyCodeNotExists()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand();
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── BusinessUnitCode Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BusinessUnitCode_Empty_FailsValidation(string code)
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitCode)
                  .WithErrorMessage("Business Unit Code is required.");
        }

        [Fact]
        public async Task BusinessUnitCode_TooLong_FailsValidation()
        {
            // 21 chars (max is 20); AlreadyExistsAsync still fires (When = !IsNullOrWhiteSpace)
            var longCode = new string('A', 21);
            var command = BusinessUnitBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitCode)
                  .WithErrorMessage("Business Unit Code cannot exceed 20 characters.");
        }

        [Theory]
        [InlineData("BU-01")]   // hyphen
        [InlineData("BU 01")]   // space
        [InlineData("BU.01")]   // dot
        [InlineData("BU_01")]   // underscore
        [InlineData("BU@01")]   // at sign
        public async Task BusinessUnitCode_NotAlphanumeric_FailsValidation(string code)
        {
            // AlreadyExistsAsync still fires even when Matches rule fails
            var command = BusinessUnitBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitCode)
                  .WithErrorMessage("Business Unit Code must be alphanumeric.");
        }

        [Fact]
        public async Task BusinessUnitCode_AlreadyExists_FailsValidation()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(code: "BU001");
            SetupCodeAlreadyExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitCode)
                  .WithErrorMessage("Business Unit Code already exists.");
        }

        [Fact]
        public async Task BusinessUnitCode_Unique_PassesExistenceCheck()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(code: "BU999");
            SetupCodeNotExists("BU999");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.BusinessUnitCode);
        }

        [Fact]
        public async Task BusinessUnitCode_MaxLength20_PassesValidation()
        {
            var maxCode = new string('A', 20);
            var command = BusinessUnitBuilders.ValidCreateCommand(code: maxCode);
            SetupCodeNotExists(maxCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.BusinessUnitCode);
        }

        // ── BusinessUnitName Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BusinessUnitName_Empty_FailsValidation(string name)
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(name: name);
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitName)
                  .WithErrorMessage("Business Unit Name is required.");
        }

        [Fact]
        public async Task BusinessUnitName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = BusinessUnitBuilders.ValidCreateCommand(name: longName);
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitName)
                  .WithErrorMessage("Business Unit Name cannot exceed 100 characters.");
        }

        [Fact]
        public async Task BusinessUnitName_MaxLength100_PassesValidation()
        {
            var maxName = new string('A', 100);
            var command = BusinessUnitBuilders.ValidCreateCommand(name: maxName);
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.BusinessUnitName);
        }

        // ── Description Rules (Optional, max 500) ────────────────────────────

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('A', 501);
            var command = BusinessUnitBuilders.ValidCreateCommand(description: longDesc);
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description cannot exceed 500 characters.");
        }

        [Fact]
        public async Task Description_Empty_PassesValidation()
        {
            var command = BusinessUnitBuilders.ValidCreateCommand(description: null);
            SetupCodeNotExists("BU001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
