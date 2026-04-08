using Contracts.Dtos.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Logistics;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Presentation.Validation.DispatchAddressMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.DispatchAddressMaster
{
    public class UpdateDispatchAddressMasterCommandValidatorTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IFreightMasterLookup> _mockFreightLookup = new(MockBehavior.Strict);

        private UpdateDispatchAddressMasterCommandValidator CreateValidator()
            => new(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockFreightLookup.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CityExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StateExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CountryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            _mockFreightLookup.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new FreightMasterLookupDto { Id = 1 });
        }

        private static UpdateDispatchAddressMasterCommand ValidCommand() => new()
        {
            Id = 1,
            DispatchAddressName = "Updated Dispatch",
            AddressLine1 = "456 Updated Street",
            CityId = 1,
            StateId = 1,
            CountryId = 1,
            PinCode = "654321",
            FreightId = 1,
            IsActive = 1
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
        public async Task DispatchAddressName_NullOrEmpty_FailsValidation(string? name)
        {
            SetupAllValid();
            var command = ValidCommand();
            command.DispatchAddressName = name;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressName);
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
        public async Task InvalidCity_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CityExistsAsync(999)).ReturnsAsync(false);
            var command = ValidCommand();
            command.CityId = 999;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId);
        }

        [Fact]
        public async Task InvalidPincode_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.PinCode = "ABC";

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PinCode);
        }

        [Fact]
        public async Task InvalidFreightId_FailsValidation()
        {
            SetupAllValid();
            _mockFreightLookup.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((FreightMasterLookupDto?)null);
            var command = ValidCommand();
            command.FreightId = 999;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FreightId);
        }

        [Fact]
        public async Task InvalidEmail_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.Email = "bad-email";

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task InvalidLatitude_FailsValidation()
        {
            SetupAllValid();
            var command = ValidCommand();
            command.Latitude = -100m;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
        }
    }
}
