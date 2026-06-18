using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Presentation.Validation.MiscTypeMaster;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandValidator CreateValidator()
            => new CreateMiscTypeMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        private void SetupCodeNotExists(string code = "QP_GROUP")
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

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MiscTypeCode_Empty_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("MiscTypeCode is required.");
        }

        [Fact]
        public async Task MiscTypeCode_TooLong_FailsValidation()
        {
            var longCode = new string('A', 21);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("MiscTypeCode  cannot be longer than   20 characters.");
        }

        [Theory]
        [InlineData("QP@GROUP")]
        [InlineData("QP.GROUP")]
        [InlineData("QP/GROUP")]
        [InlineData("QP_GROUP")]
        [InlineData("QP-GROUP")]
        [InlineData("QP GROUP")]
        public async Task MiscTypeCode_NonAlphanumeric_FailsValidation(string code)
        {
            // MiscTypeCode uses the Alphanumeric rule (^[A-Za-z0-9]+$) — spaces, separators
            // and special characters are rejected (consistent with other modules' code fields).
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("MiscTypeCode  must be alphanumeric only.");
        }

        [Theory]
        [InlineData("QP01")]
        [InlineData("123ABC")]
        [InlineData("QPGROUP")]
        [InlineData("PHYSICAL")]
        public async Task MiscTypeCode_Alphanumeric_PassesPatternCheck(string code)
        {
            // Letters and digits are valid; no separators needed.
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);
            SetupCodeNotExists(code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task MiscTypeCode_AlreadyExists_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "QP_GROUP");
            SetupCodeAlreadyExists("QP_GROUP");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                  .WithErrorMessage("MiscTypeCode already exists.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: description);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   250 characters.");
        }
    }
}
