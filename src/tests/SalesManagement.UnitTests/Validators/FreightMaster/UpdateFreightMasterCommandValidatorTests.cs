using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using SalesManagement.Presentation.Validation.FreightMaster;

namespace SalesManagement.UnitTests.Validators.FreightMaster
{
    public sealed class UpdateFreightMasterCommandValidatorTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateFreightMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int freightModeId = 1, int rateMethodId = 2)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(freightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(rateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(freightModeId, rateMethodId, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(freightModeId, rateMethodId)).ReturnsAsync(true);
        }

        private static UpdateFreightMasterCommand ValidCommand() => new()
        {
            Id = 1,
            FreightModeId = 1,
            RateMethodId = 2,
            Rate = 150m,
            IsActive = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 2, 1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(1, 2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
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

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
