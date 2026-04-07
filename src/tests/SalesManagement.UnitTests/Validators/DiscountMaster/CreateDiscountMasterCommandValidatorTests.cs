using Contracts.Interfaces.Lookups.Purchase;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public sealed class CreateDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPaymentTermLookup> _mockPaymentTermLookup = new(MockBehavior.Strict);

        private CreateDiscountMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockPaymentTermLookup.Object);

        private void SetupAllAsyncMocks(string name = "Test Discount")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Purchase.PaymentTermLookupDto>());
        }

        private static CreateDiscountMasterCommand ValidCommand() => new()
        {
            DiscountName = "Test Discount",
            DiscountTypeId = 1,
            ApplicableLevelId = 2,
            TriggerEventId = 3,
            ValueTypeId = 4,
            DiscountValue = 10.5m
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── DiscountName Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DiscountName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.DiscountName = name;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DiscountName);
        }

        [Fact]
        public async Task DiscountName_AlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Test Discount", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Purchase.PaymentTermLookupDto>());

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DiscountName);
        }

        // ── FK Required Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task DiscountTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.DiscountTypeId = 0;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Test Discount", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DiscountTypeId);
        }

        [Fact]
        public async Task ApplicableLevelId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.ApplicableLevelId = 0;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Test Discount", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ApplicableLevelId);
        }

        [Fact]
        public async Task ValueTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.ValueTypeId = 0;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Test Discount", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ValueTypeId);
        }
    }
}
