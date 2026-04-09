using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Presentation.Validation.AssetMaster.AssetWaranty;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetWarranty
{
    public sealed class UpdateAssetWarrantyCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateAssetWarrantyCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static UpdateAssetWarrantyCommand ValidCommand() =>
            new UpdateAssetWarrantyCommand
            {
                Id = 1,
                AssetId = 1,
                Period = 12,
                WarrantyType = 1,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                ContactPerson = "John",
                MobileNumber = "9876543210",
                Email = "john@test.com",
                ServiceCountryId = 1,
                ServiceStateId = 1,
                ServiceCityId = 1,
                ServicePinCode = "123456",
                ServiceMobileNumber = "9876543210",
                ServiceEmail = "service@test.com",
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullPeriod_FailsValidation()
        {
            var command = ValidCommand();
            command.Period = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InvalidMobileNumber_FailsValidation()
        {
            var command = ValidCommand();
            command.MobileNumber = "12345";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InvalidEmail_FailsValidation()
        {
            var command = ValidCommand();
            command.Email = "not-an-email";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
