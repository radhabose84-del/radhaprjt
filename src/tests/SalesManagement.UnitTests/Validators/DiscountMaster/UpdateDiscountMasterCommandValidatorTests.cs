using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public class UpdateDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPaymentTermLookup> _mockPaymentTermLookup = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Strict);

        private UpdateDiscountMasterCommandValidator CreateValidator()
            => new(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockPaymentTermLookup.Object,
                _mockCurrencyLookup.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockPaymentTermLookup.Setup(r => r.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<PaymentTermLookupDto> { new() { Id = 1, Description = "Net30" } });
            _mockCurrencyLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto> { new() { CurrencyId = 1 } });
        }

        private static UpdateDiscountMasterCommand ValidCommand() => new()
        {
            Id = 1,
            DiscountName = "Updated Discount",
            TriggerEventId = 1,
            DiscountBasisId = 2,
            ExecutionTypeId = 3,
            ValueTypeId = 4,
            SlabTypeId = 5,
            Priority = 1,
            IsActive = 1,
            Slabs = new List<DiscountSlabItem>
            {
                new() { SlabOrder = 1, FromValue = 0, ToValue = 100, DiscountValue = 10 }
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
        public async Task NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = ValidCommand();
            command.Id = 99;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task IsActive_OutOfRange_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.IsActive = 5;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
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
        public async Task InvalidMiscMaster_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);
            var command = ValidCommand();
            command.TriggerEventId = 999;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TriggerEventId);
        }
    }
}
