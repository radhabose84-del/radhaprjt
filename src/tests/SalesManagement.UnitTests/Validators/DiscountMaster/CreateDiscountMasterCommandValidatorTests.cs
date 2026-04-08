using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public class CreateDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPaymentTermLookup> _mockPaymentTermLookup = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Strict);

        private CreateDiscountMasterCommandValidator CreateValidator()
            => new(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockPaymentTermLookup.Object,
                _mockCurrencyLookup.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<PaymentTermLookupDto> { new() { Id = 1, Description = "Net30" } });
            _mockCurrencyLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto> { new() { CurrencyId = 1 } });
        }

        private static CreateDiscountMasterCommand ValidCommand() => new()
        {
            DiscountName = "Test Discount",
            TriggerEventId = 1,
            DiscountBasisId = 2,
            ExecutionTypeId = 3,
            ValueTypeId = 4,
            SlabTypeId = 5,
            Priority = 1,
            Slabs = new List<DiscountSlabItem>
            {
                new() { SlabOrder = 1, FromValue = 0, ToValue = 100, DiscountValue = 5 }
            }
        };

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DiscountName_NullOrEmpty_FailsValidation(string? name)
        {
            SetupAllValid();
            var command = ValidCommand();
            command.DiscountName = name;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DiscountName);
        }

        [Fact]
        public async Task TriggerEventId_Zero_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.TriggerEventId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TriggerEventId);
        }

        [Fact]
        public async Task DuplicateName_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Existing Discount", null)).ReturnsAsync(true);
            var command = ValidCommand();
            command.DiscountName = "Existing Discount";

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DiscountName);
        }

        [Fact]
        public async Task InvalidMiscMaster_TriggerEventId_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);
            var command = ValidCommand();
            command.TriggerEventId = 999;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TriggerEventId);
        }

        [Fact]
        public async Task EmptySlabs_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.Slabs = new List<DiscountSlabItem>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Slabs);
        }

        [Fact]
        public async Task Priority_Zero_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.Priority = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Priority);
        }
    }
}
