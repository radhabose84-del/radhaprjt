using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Presentation.Validation.DispatchAddressMaster;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DispatchAddressMaster
{
    public class UpdateDispatchAddressMasterCommandValidatorTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateDispatchAddressMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        /// <summary>
        /// Sets up all async mocks to pass (valid state).
        /// </summary>
        private void SetupAllAsyncMocksPass(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
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
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocksPass(id: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotFound Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CityExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StateExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(id: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CityExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StateExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Dispatch Address Master not found.");
        }

        // ── DispatchAddressName Rules ──────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DispatchAddressName_Empty_FailsValidation(string? name)
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(dispatchAddressName: name);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName is required.");
        }

        [Fact]
        public async Task DispatchAddressName_TooLong_FailsValidation()
        {
            var longName = new string('A', 151);
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(dispatchAddressName: longName);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName  cannot be longer than   150 characters.");
        }

        // ── AlreadyExists (composite key with exclude self) ────────────────────

        [Fact]
        public async Task DispatchAddressName_AlreadyExistsForAnotherRecord_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(
                id: 1,
                dispatchAddressName: "Existing Address",
                cityId: 1,
                pinCode: "110001");
            SetupAllAsyncMocksPass(id: 1);
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync("Existing Address", 1, "110001", 1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName)
                  .WithErrorMessage("DispatchAddressName already exists.");
        }

        // ── FK Rules ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CityId_DoesNotExist_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(cityId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.CityExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId);
        }

        [Fact]
        public async Task StateId_DoesNotExist_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(stateId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.StateExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateId);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValue_PassesValidation(int isActive)
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        // ── Optional Field Rules ──────────────────────────────────────────────

        [Fact]
        public async Task MobileNumber_InvalidFormat_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            command.MobileNumber = "12345";
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MobileNumber)
                  .WithErrorMessage("MobileNumber must be a 10-digit numeric value.");
        }

        [Fact]
        public async Task Latitude_OutOfRange_FailsValidation()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            command.Latitude = 95m;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Latitude)
                  .WithErrorMessage("Latitude must be between -90 and 90.");
        }
    }
}
