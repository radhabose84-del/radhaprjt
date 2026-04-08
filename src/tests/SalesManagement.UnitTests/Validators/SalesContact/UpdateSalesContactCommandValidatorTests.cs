using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using SalesManagement.Presentation.Validation.SalesContact;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesContact
{
    public sealed class UpdateSalesContactCommandValidatorTests
    {
        private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);

        private UpdateSalesContactCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockPartyLookup.Object);

        private void SetupAllAsyncMocks(int id = 1, string mobile = "9876543210", int contactTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(contactTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync(mobile!, id)).ReturnsAsync(false);
        }

        private static UpdateSalesContactCommand ValidCommand() => new()
        {
            Id = 1,
            ContactName = "Test Contact",
            MobileNumber = "9876543210",
            ContactTypeId = 1,
            IsActive = 1
        };

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", id)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", 1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ContactName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.ContactName = name;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ContactName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MobileNumber_Empty_FailsValidation(string? mobile)
        {
            var cmd = ValidCommand();
            cmd.MobileNumber = mobile;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        [Fact]
        public async Task DuplicateMobile_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", 1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

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
