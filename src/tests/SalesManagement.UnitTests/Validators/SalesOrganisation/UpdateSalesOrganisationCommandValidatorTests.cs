using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using SalesManagement.Presentation.Validation.SalesOrganisation;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrganisation
{
    public class UpdateSalesOrganisationCommandValidatorTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesOrganisationCommandValidator CreateValidator()
            => new UpdateSalesOrganisationCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupIdExists(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
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

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, companyId: 1);
            SetupIdExists(1);
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: id, companyId: 1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        // ── SalesOrganisationName Rules ───────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesOrganisationName_Empty_FailsValidation(string? name)
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, name: name, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, name: longName, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, name: maxName, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, companyId: companyId);
            SetupIdExists(1);

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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, companyId: 999);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, description: longDesc, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, description: null, companyId: 1);
            SetupIdExists(1);
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
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1, description: maxDesc, companyId: 1);
            SetupIdExists(1);
            SetupCompanyExists(1);

            var validator = CreateValidator();

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        // ── Immutability Verification ─────────────────────────────────────────

        [Fact]
        public void UpdateCommand_DoesNotContain_SalesOrganisationCode_Property()
        {
            // Assert — SalesOrganisationCode must NOT be in the Update command (immutable field)
            var commandType = typeof(UpdateSalesOrganisationCommand);
            var codeProperty = commandType.GetProperty("SalesOrganisationCode");

            codeProperty.Should().BeNull(
                "SalesOrganisationCode is immutable and must NOT be included in the UpdateSalesOrganisationCommand");
        }
    }
}
