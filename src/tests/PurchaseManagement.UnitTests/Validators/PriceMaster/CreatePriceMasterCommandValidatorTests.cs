using FluentValidation.TestHelper;
using PurchaseManagement.Application.PriceMaster.Commands.Create;
using PurchaseManagement.Application.PriceMaster.Dtos;

namespace PurchaseManagement.UnitTests.Validators.PriceMaster
{
    public sealed class CreatePriceMasterCommandValidatorTests
    {
        private CreatePriceMasterCommandValidator CreateValidator() => new();

        private static PriceMasterDetailUpsertDto ValidDetail(decimal from = 1, decimal? to = null) =>
            new() { ScaleQtyFrom = from, ScaleQtyTo = to, UnitPrice = 100m, CurrencyId = 1 };

        private static CreatePriceMasterCommand ValidCommand() => new()
        {
            Data = new PriceMasterCreateDto
            {
                ItemId = 1,
                VendorId = 1,
                UomId = 1,
                ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                Details = new List<PriceMasterDetailUpsertDto> { ValidDetail() }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullData_FailsValidation()
        {
            var command = new CreatePriceMasterCommand { Data = null! };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public async Task Validate_ZeroItemId_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.ItemId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroVendorId_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.VendorId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.Details = new List<PriceMasterDetailUpsertDto>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ValidToBeforeValidFrom_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.ValidFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            command.Data.ValidTo = DateOnly.FromDateTime(DateTime.Today);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_OverlappingTiers_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.Details = new List<PriceMasterDetailUpsertDto>
            {
                ValidDetail(from: 1, to: 100),
                ValidDetail(from: 50, to: 200)  // overlaps with previous
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroUnitPrice_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.Details = new List<PriceMasterDetailUpsertDto>
            {
                new() { ScaleQtyFrom = 1, UnitPrice = 0, CurrencyId = 1 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
