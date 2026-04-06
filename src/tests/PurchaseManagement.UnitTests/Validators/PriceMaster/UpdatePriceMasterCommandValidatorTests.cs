using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Update;
using PurchaseManagement.Application.PriceMaster.Dtos;

namespace PurchaseManagement.UnitTests.Validators.PriceMaster
{
    public sealed class UpdatePriceMasterCommandValidatorTests
    {
        private readonly Mock<IPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdatePriceMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private static PriceMasterDetailUpsertDto ValidDetail(decimal from = 1, decimal? to = null) =>
            new() { ScaleQtyFrom = from, ScaleQtyTo = to, UnitPrice = 100m, CurrencyId = 1 };

        private static UpdatePriceMasterCommand ValidCommand() => new()
        {
            Data = new PriceMasterUpdateDto
            {
                Id = 1,
                ItemId = 1,
                VendorId = 1,
                UomId = 1,
                ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                IsActive = 1,
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
            var command = new UpdatePriceMasterCommand { Data = null! };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
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
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.Data.Details = new List<PriceMasterDetailUpsertDto>();

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
                ValidDetail(from: 50, to: 200)  // overlaps
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
