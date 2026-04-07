using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using SalesManagement.Presentation.Validation.FreightMaster;

namespace SalesManagement.UnitTests.Validators.FreightMaster
{
    public sealed class CreateFreightMasterCommandValidatorTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateFreightMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int freightModeId = 1, int rateMethodId = 2)
        {
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(freightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(rateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(freightModeId, rateMethodId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(freightModeId, rateMethodId)).ReturnsAsync(true);
        }

        private static CreateFreightMasterCommand ValidCommand() => new()
        {
            FreightModeId = 1,
            RateMethodId = 2,
            Rate = 100m
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── FreightModeId Rules ───────────────────────────────────────────────

        [Fact]
        public async Task FreightModeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.FreightModeId = 0;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.FreightModeId);
        }

        // ── RateMethodId Rules ────────────────────────────────────────────────

        [Fact]
        public async Task RateMethodId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.RateMethodId = 0;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.RateMethodId);
        }

        // ── Rate Rules ────────────────────────────────────────────────────────

        [Fact]
        public async Task Rate_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Rate = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        // ── CompositeKey AlreadyExists Rules ──────────────────────────────────

        [Fact]
        public async Task CompositeKey_AlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(2)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 2, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(1, 2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
