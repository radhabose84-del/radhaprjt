using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Presentation.Validation.DispatchAddressMaster;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DispatchAddressMaster
{
    public class CreateDispatchAddressMasterCommandValidatorTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateDispatchAddressMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        /// <summary>
        /// Sets up all async mocks to pass (valid state).
        /// Specific tests override individual mocks as needed.
        /// </summary>
        private void SetupAllAsyncMocksPass()
        {
            _mockQueryRepo.Setup(r => r.CityExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StateExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── DispatchAddressName Rules ──────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DispatchAddressName_Empty_FailsValidation(string? name)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(dispatchAddressName: name);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName is required.");
        }

        [Fact]
        public async Task DispatchAddressName_TooLong_FailsValidation()
        {
            var longName = new string('A', 151);
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(dispatchAddressName: longName);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName  cannot be longer than   150 characters.");
        }

        [Fact]
        public async Task DispatchAddressName_AlreadyExists_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(
                dispatchAddressName: "Duplicate Address",
                cityId: 1,
                pinCode: "110001");
            SetupAllAsyncMocksPass();
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync("Duplicate Address", 1, "110001", null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName already exists.");
        }

        // ── AddressLine1 Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AddressLine1_Empty_FailsValidation(string? addressLine1)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(addressLine1: addressLine1);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AddressLine1)
                  .WithErrorMessage("AddressLine1 is required.");
        }

        [Fact]
        public async Task AddressLine1_TooLong_FailsValidation()
        {
            var longLine = new string('B', 251);
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(addressLine1: longLine);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AddressLine1)
                  .WithErrorMessage("AddressLine1  cannot be longer than   250 characters.");
        }

        // ── PinCode Rules ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PinCode_Empty_FailsValidation(string? pinCode)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(pinCode: pinCode);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PinCode)
                  .WithErrorMessage("PinCode is required.");
        }

        [Theory]
        [InlineData("12345")]    // 5 digits
        [InlineData("1234567")]  // 7 digits
        [InlineData("ABCDEF")]   // letters
        [InlineData("11000 1")]  // space
        public async Task PinCode_InvalidFormat_FailsValidation(string pinCode)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(pinCode: pinCode);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PinCode)
                  .WithErrorMessage("PinCode must be a 6-digit numeric value.");
        }

        [Fact]
        public async Task PinCode_ValidSixDigits_PassesValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(pinCode: "110001");
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.PinCode);
        }

        // ── CityId / StateId / CountryId Rules ────────────────────────────────

        [Fact]
        public async Task CityId_Zero_FailsNotEmpty()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(cityId: 0);
            _mockQueryRepo.Setup(r => r.StateExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId)
                  .WithErrorMessage("CityId is required.");
        }

        [Fact]
        public async Task CityId_DoesNotExist_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(cityId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.CityExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId);
        }

        [Fact]
        public async Task StateId_DoesNotExist_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(stateId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.StateExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateId);
        }

        [Fact]
        public async Task CountryId_DoesNotExist_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand(countryId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CountryId);
        }

        // ── Optional Field Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData("98765432")]    // 8 digits
        [InlineData("987654321012")] // 12 digits
        [InlineData("ABCDEFGHIJ")]  // letters
        public async Task MobileNumber_InvalidFormat_FailsValidation(string mobile)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.MobileNumber = mobile;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MobileNumber)
                  .WithErrorMessage("MobileNumber must be a 10-digit numeric value.");
        }

        [Fact]
        public async Task Email_InvalidFormat_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.Email = "not-an-email";
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email must be a valid email address.");
        }

        [Fact]
        public async Task GSTIN_InvalidFormat_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.GSTIN = "INVALIDGSTIN";
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GSTIN)
                  .WithErrorMessage("GSTIN must be a valid 15-character GSTIN format.");
        }

        [Fact]
        public async Task GSTIN_ValidFormat_PassesValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.GSTIN = "22AAAAA0000A1Z5";
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.GSTIN);
        }

        [Theory]
        [InlineData(91.0)]
        [InlineData(-91.0)]
        public async Task Latitude_OutOfRange_FailsValidation(double lat)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.Latitude = (decimal)lat;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Latitude)
                  .WithErrorMessage("Latitude must be between -90 and 90.");
        }

        [Theory]
        [InlineData(181.0)]
        [InlineData(-181.0)]
        public async Task Longitude_OutOfRange_FailsValidation(double lon)
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            command.Longitude = (decimal)lon;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Longitude)
                  .WithErrorMessage("Longitude must be between -180 and 180.");
        }
    }
}
