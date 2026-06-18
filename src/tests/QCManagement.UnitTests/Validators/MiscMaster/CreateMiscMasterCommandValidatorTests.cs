using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Presentation.Validation.MiscMaster;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.MiscMaster
{
    public class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscMasterCommandValidator CreateValidator()
            => new CreateMiscMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        private void SetupHappyPath(string code = "PHY", int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(miscTypeId)).ReturnsAsync(true);
        }

        private void SetupAnyCodeOkAndTypeExists()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Code_Empty_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code is required.");
        }

        [Fact]
        public async Task Code_TooLong_FailsValidation()
        {
            var longCode = new string('A', 21);
            var command = MiscMasterBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeOkAndTypeExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code  cannot be longer than   20 characters.");
        }

        [Theory]
        [InlineData("PHY@01")]
        [InlineData("PHY.01")]
        [InlineData("PHY GROUP")]
        [InlineData("PHY-GROUP")]
        [InlineData("PHY_GROUP")]
        public async Task Code_NonAlphanumeric_FailsValidation(string code)
        {
            // Code uses the Alphanumeric rule (^[A-Za-z0-9]+$) — spaces, separators and
            // special characters are rejected (consistent with every other module's code field).
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeOkAndTypeExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code  must be alphanumeric only.");
        }

        [Theory]
        [InlineData("PHY")]
        [InlineData("PHY01")]
        [InlineData("PHY123")]
        [InlineData("123")]
        public async Task Code_Alphanumeric_PassesPatternCheck(string code)
        {
            // Letters and digits are valid; no spaces/separators needed.
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            SetupHappyPath(code, 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Code_AlreadyExistsWithinType_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "PHY", miscTypeId: 1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("PHY", 1, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code already exists.");
        }

        [Fact]
        public async Task MiscTypeId_NotInDb_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 999);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), 999, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   250 characters.");
        }
    }
}
