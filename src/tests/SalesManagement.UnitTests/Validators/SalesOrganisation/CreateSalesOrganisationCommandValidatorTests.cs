using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Presentation.Validation.SalesOrganisation;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrganisation
{
    public class CreateSalesOrganisationCommandValidatorTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesOrganisationCommandValidator CreateValidator()
            => new CreateSalesOrganisationCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupCodeNotExists(string code = "ORG001")
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

        private void SetupCompanyExists(int companyId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.CompanyExistsAsync(companyId))
                .ReturnsAsync(true);
        }

        private void SetupCompanyNotExists(int companyId)
        {
            _mockQueryRepo
                .Setup(r => r.CompanyExistsAsync(companyId))
                .ReturnsAsync(false);
        }

        /// <summary>
        /// Use when the test doesn't care about uniqueness — allows any code through the existence check.
        /// Needed because FluentValidation runs ALL rules regardless of earlier failures.
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
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand();
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SalesOrganisationCode Rules ───────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesOrganisationCode_Empty_FailsValidation(string? code)
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: code);
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationCode)
                  .WithErrorMessage("SalesOrganisationCode is required.");
        }

        [Fact]
        public async Task SalesOrganisationCode_TooLong_FailsValidation()
        {
            // Arrange — 21 characters (max is 20)
            // AlreadyExistsAsync still fires (When condition is only on whitespace)
            var longCode = new string('A', 21);
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeNotExists();
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationCode)
                  .WithErrorMessage("SalesOrganisationCode  cannot be longer than   20 characters.");
        }

        [Theory]
        [InlineData("ABC-123")]   // hyphen
        [InlineData("ABC 123")]   // space
        [InlineData("ABC.123")]   // dot
        [InlineData("ABC_123")]   // underscore
        [InlineData("ABC@123")]   // at sign
        public async Task SalesOrganisationCode_NotAlphanumeric_FailsValidation(string code)
        {
            // Arrange
            // AlreadyExistsAsync still fires even when Matches rule fails
            // (FluentValidation runs all rules; When guards only on !IsNullOrWhiteSpace)
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeNotExists();
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationCode)
                  .WithErrorMessage("SalesOrganisationCode  must be alphanumeric only.");
        }

        [Fact]
        public async Task SalesOrganisationCode_AlreadyExists_FailsValidation()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: "ORG001");
            SetupCodeAlreadyExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationCode)
                  .WithErrorMessage("SalesOrganisationCode already exists.");
        }

        [Fact]
        public async Task SalesOrganisationCode_Unique_PassesExistenceCheck()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: "ORG999");
            SetupCodeNotExists("ORG999");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert — no uniqueness error
            result.ShouldNotHaveValidationErrorFor(x => x.SalesOrganisationCode);
        }

        [Fact]
        public async Task SalesOrganisationCode_MaxLength20_PassesValidation()
        {
            // Arrange — exactly 20 characters
            var maxCode = new string('A', 20);
            var command = SalesOrganisationBuilders.ValidCreateCommand(code: maxCode);
            SetupCodeNotExists(maxCode);
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SalesOrganisationCode);
        }

        // ── SalesOrganisationName Rules ───────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesOrganisationName_Empty_FailsValidation(string? name)
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(name: name);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationName)
                  .WithErrorMessage("SalesOrganisationName is required.");
        }

        [Fact]
        public async Task SalesOrganisationName_TooLong_FailsValidation()
        {
            // Arrange — 101 characters (max is 100)
            var longName = new string('A', 101);
            var command = SalesOrganisationBuilders.ValidCreateCommand(name: longName);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationName)
                  .WithErrorMessage("SalesOrganisationName  cannot be longer than   100 characters.");
        }

        [Fact]
        public async Task SalesOrganisationName_MaxLength100_PassesValidation()
        {
            // Arrange — exactly 100 characters
            var maxName = new string('A', 100);
            var command = SalesOrganisationBuilders.ValidCreateCommand(name: maxName);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SalesOrganisationName);
        }

        // ── CompanyId Rules ───────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task CompanyId_ZeroOrNegative_FailsValidation(int companyId)
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(companyId: companyId);
            SetupCodeNotExists("ORG001");

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                  .WithErrorMessage("CompanyId is required.");
        }

        [Fact]
        public async Task CompanyId_DoesNotExist_FailsValidation()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(companyId: 999);
            SetupCodeNotExists("ORG001");
            SetupCompanyNotExists(999);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CompanyId)
                  .WithErrorMessage("CompanyId Company Id is inactive/deleted.");
        }

        [Fact]
        public async Task CompanyId_Exists_PassesValidation()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidCreateCommand(companyId: 1);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CompanyId);
        }

        // ── Description Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            // Arrange — 501 characters (max is 500)
            var longDesc = new string('X', 501);
            var command = SalesOrganisationBuilders.ValidCreateCommand(description: longDesc);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   500 characters.");
        }

        [Fact]
        public async Task Description_Empty_PassesValidation()
        {
            // Arrange — description is optional
            var command = SalesOrganisationBuilders.ValidCreateCommand(description: null);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert — no error for empty description
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Description_MaxLength500_PassesValidation()
        {
            // Arrange — exactly 500 characters
            var maxDesc = new string('X', 500);
            var command = SalesOrganisationBuilders.ValidCreateCommand(description: maxDesc);
            SetupCodeNotExists("ORG001");
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
