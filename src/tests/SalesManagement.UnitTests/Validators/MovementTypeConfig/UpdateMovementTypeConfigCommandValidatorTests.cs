using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Presentation.Validation.MovementTypeConfig;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MovementTypeConfig
{
    public sealed class UpdateMovementTypeConfigCommandValidatorTests
    {
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMovementTypeConfigCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int categoryId = 1, int fromStockTypeId = 1, int toStockTypeId = 2)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(categoryId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(fromStockTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(toStockTypeId)).ReturnsAsync(true);
        }

        private static UpdateMovementTypeConfigCommand ValidCommand() => new()
        {
            Id = 1,
            MovementDescription = "Updated Movement",
            MovementCategoryId = 1,
            FromStockTypeId = 1,
            ToStockTypeId = 2,
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── MovementDescription Rules ─────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MovementDescription_Empty_FailsValidation(string? description)
        {
            var cmd = ValidCommand();
            cmd.MovementDescription = description;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MovementDescription);
        }

        // ── FK Rules ─────────────────────────────────────────────────────────

        [Fact]
        public async Task MovementCategoryId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.MovementCategoryId = 0;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MovementCategoryId);
        }

        // ── Cross-field: FromStockTypeId != ToStockTypeId ─────────────────────

        [Fact]
        public async Task SameFromAndToStockType_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.FromStockTypeId = 5;
            cmd.ToStockTypeId = 5;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(5)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ToStockTypeId);
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

        // ── ValueUpdateFlag conditional ───────────────────────────────────────

        [Fact]
        public async Task ValueUpdateFlag_True_NoAccountModifier_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.ValueUpdateFlag = true;
            cmd.AccountModifier = null;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.AccountModifier);
        }
    }
}
