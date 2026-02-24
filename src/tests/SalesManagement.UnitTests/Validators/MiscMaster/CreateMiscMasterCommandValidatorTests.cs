#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using SalesManagement.Presentation.Validation.MiscMaster;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MiscMaster
{
    public class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        // Setup helpers

        private void SetupMiscTypeExists(int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(miscTypeId)).ReturnsAsync(true);
        }

        private void SetupMiscTypeNotExists(int miscTypeId)
        {
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(miscTypeId)).ReturnsAsync(false);
        }

        private void SetupCodeNotExists(string code = "CODE001", int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(false);
        }

        private void SetupCodeAlreadyExists(string code, int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(true);
        }

        private void SetupAnyCodeNotExists(int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), miscTypeId, null)).ReturnsAsync(false);
        }

        private void SetupAllAsyncMocks(string code = "CODE001", int miscTypeId = 1)
        {
            SetupMiscTypeExists(miscTypeId);
            SetupCodeNotExists(code, miscTypeId);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── MiscTypeId Rules ──────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task MiscTypeId_ZeroOrNegative_FailsValidation(int miscTypeId)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: miscTypeId);
            // When MiscTypeId <= 0, neither async mock fires

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId)
                  .WithErrorMessage("Valid Misc Type is required.");
        }

        [Fact]
        public async Task MiscTypeId_DoesNotExist_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 999, code: "CODE001");
            SetupMiscTypeNotExists(999);
            SetupCodeNotExists("CODE001", 999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId)
                  .WithErrorMessage("Misc Type does not exist in Misc Type Master.");
        }

        [Fact]
        public async Task MiscTypeId_Exists_PassesMiscTypeCheck()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeId);
        }

        // ── Code Rules ────────────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Code_Empty_FailsValidation(string code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            // AlreadyExistsAsync won't fire (code is empty), only MiscTypeExistsAsync fires
            SetupMiscTypeExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code is required.");
        }

        [Fact]
        public async Task Code_TooLong_FailsValidation()
        {
            var longCode = new string('A', 21);
            var command = MiscMasterBuilders.ValidCreateCommand(code: longCode);
            SetupMiscTypeExists(1);
            SetupAnyCodeNotExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code cannot exceed 20 characters.");
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        [InlineData("CODE.01")]
        public async Task Code_NotAlphanumeric_FailsValidation(string code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            SetupMiscTypeExists(1);
            SetupAnyCodeNotExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code must be alphanumeric only.");
        }

        [Fact]
        public async Task Code_AlreadyExists_ForSameMiscType_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "CODE001");
            SetupMiscTypeExists(1);
            SetupCodeAlreadyExists("CODE001", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Code already exists for this Misc Type.");
        }

        [Fact]
        public async Task Code_Unique_PassesExistenceCheck()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "CODE999");
            SetupMiscTypeExists(1);
            SetupCodeNotExists("CODE999", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Code_MaxLength20_PassesValidation()
        {
            var maxCode = new string('A', 20);
            var command = MiscMasterBuilders.ValidCreateCommand(code: maxCode);
            SetupMiscTypeExists(1);
            SetupCodeNotExists(maxCode, 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Code);
        }

        // ── Description Rules ─────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string description)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description cannot exceed 250 characters.");
        }

        [Fact]
        public async Task Description_MaxLength250_PassesValidation()
        {
            var maxDesc = new string('X', 250);
            var command = MiscMasterBuilders.ValidCreateCommand(description: maxDesc);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
