using FluentValidation.TestHelper;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Presentation.Validation.BusinessUnit;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.BusinessUnit
{
    /// <summary>
    /// ⚠️ UpdateBusinessUnitCommandValidator has two special rules vs SalesChannel:
    /// 1. Id has a NotFoundAsync existence check (not just GreaterThan(0))
    /// 2. IsActive must be 0 or 1 (validated)
    /// </summary>
    public class UpdateBusinessUnitCommandValidatorTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateBusinessUnitCommandValidator CreateValidator()
            => new UpdateBusinessUnitCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupIdExists(int id = 1)
        {
            // NotFoundAsync returns false when entity IS found (not-not-found = found)
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
        }

        private void SetupIdNotFound(int id = 99)
        {
            // NotFoundAsync returns true when entity is NOT found
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        private void SetupNameNotExists()
        {
            _mockQueryRepo
                .Setup(r => r.NameAlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            // When Id <= 0, GreaterThan(0) fails — NotFoundAsync is still called (chained rule)
            // We need to set up NotFoundAsync for the chained MustAsync rule
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true); // entity not found (irrelevant, just satisfies strict mock)
            SetupNameNotExists();

            var command = BusinessUnitBuilders.ValidUpdateCommand(id: id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Business Unit ID is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 99);
            SetupIdNotFound(99);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Business Unit not found.");
        }

        // ── BusinessUnitName Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BusinessUnitName_Empty_FailsValidation(string? name)
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, name: name);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitName)
                  .WithErrorMessage("BusinessUnitName is required.");
        }

        [Fact]
        public async Task BusinessUnitName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, name: longName);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitName)
                  .WithErrorMessage("BusinessUnitName  cannot be longer than   100 characters.");
        }

        // ── Description Rules (Optional, max 500) ────────────────────────────

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('A', 501);
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, description: longDesc);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   500 characters.");
        }

        [Fact]
        public async Task Description_Empty_PassesValidation()
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, description: null);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, isActive: isActive);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = BusinessUnitBuilders.ValidUpdateCommand(id: 1, isActive: isActive);
            SetupIdExists(1);
            SetupNameNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        // ── Immutability Verification ─────────────────────────────────────────

        [Fact]
        public void UpdateCommand_DoesNotContain_BusinessUnitCode_Property()
        {
            // BusinessUnitCode is immutable and must NOT be in the Update command
            var commandType = typeof(UpdateBusinessUnitCommand);
            var codeProperty = commandType.GetProperty("BusinessUnitCode");

            codeProperty.Should().BeNull(
                "BusinessUnitCode is immutable and must NOT be included in UpdateBusinessUnitCommand");
        }
    }
}
