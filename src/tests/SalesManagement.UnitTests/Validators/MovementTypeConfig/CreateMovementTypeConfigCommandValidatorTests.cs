using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Presentation.Validation.MovementTypeConfig;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MovementTypeConfig
{
    public sealed class CreateMovementTypeConfigCommandValidatorTests
    {
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMovementTypeConfigCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(
            string code = "MOVE",
            int movementCategoryId = 1,
            int fromStockTypeId = 1,
            int toStockTypeId = 2)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(movementCategoryId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(fromStockTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(toStockTypeId)).ReturnsAsync(true);
        }

        private static CreateMovementTypeConfigCommand ValidCommand() => new()
        {
            MovementCode = "MOVE",
            MovementDescription = "Test Movement",
            MovementCategoryId = 1,
            FromStockTypeId = 1,
            ToStockTypeId = 2,
            QuantityUpdateFlag = true,
            ValueUpdateFlag = false
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── MovementCode Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MovementCode_Empty_FailsValidation(string? code)
        {
            var cmd = ValidCommand();
            cmd.MovementCode = code;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MovementCode);
        }

        [Theory]
        [InlineData("CODE-1")]
        [InlineData("CODE 1")]
        [InlineData("CODE@1")]
        public async Task MovementCode_NonAlphanumeric_FailsValidation(string code)
        {
            var cmd = ValidCommand();
            cmd.MovementCode = code;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MovementCode);
        }

        [Fact]
        public async Task MovementCode_AlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MOVE", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MovementCode);
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
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MOVE", null)).ReturnsAsync(false);
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
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MOVE", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(5)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ToStockTypeId);
        }

        // ── ValueUpdateFlag conditional: AccountModifier required ─────────────

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

        [Fact]
        public async Task ValueUpdateFlag_True_WithAccountModifier_PassesValidation()
        {
            var cmd = ValidCommand();
            cmd.ValueUpdateFlag = true;
            cmd.AccountModifier = "ACC001";
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.AccountModifier);
        }
    }
}
