using Contracts.Interfaces.Lookups.Purchase;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public sealed class UpdateDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPaymentTermLookup> _mockPaymentTermLookup = new(MockBehavior.Strict);

        private UpdateDiscountMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockPaymentTermLookup.Object);

        private void SetupAllAsyncMocks(int id = 1, string name = "Updated Discount")
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Purchase.PaymentTermLookupDto>());
        }

        private static UpdateDiscountMasterCommand ValidCommand() => new()
        {
            Id = 1,
            DiscountName = "Updated Discount",
            DiscountTypeId = 1,
            ApplicableLevelId = 2,
            TriggerEventId = 3,
            ValueTypeId = 4,
            DiscountValue = 20m,
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
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Updated Discount", 1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Purchase.PaymentTermLookupDto>());

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── DiscountName Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DiscountName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.DiscountName = name;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DiscountName);
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
