using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.CreateSalesContact;
using SalesManagement.Presentation.Validation.SalesContact;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesContact
{
    public sealed class CreateSalesContactCommandValidatorTests
    {
        private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CreateSalesContactCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockPartyLookup.Object, _mockAccessFilter.Object);

        private void SetupAllAsyncMocks(string mobile = "9876543210", int contactTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync(mobile, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(contactTypeId)).ReturnsAsync(true);
        }

        private static CreateSalesContactCommand ValidCommand() => new()
        {
            ContactName = "John Doe",
            MobileNumber = "9876543210",
            ContactTypeId = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── ContactName Rules ─────────────────────────────────────────────────

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

        // ── MobileNumber Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task MobileNumber_Empty_FailsValidation(string? mobile)
        {
            var cmd = ValidCommand();
            cmd.MobileNumber = mobile;
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        [Theory]
        [InlineData("98765432")]     // 8 digits
        [InlineData("987654321012")] // 12 digits
        [InlineData("ABCDEFGHIJ")]   // letters
        public async Task MobileNumber_InvalidFormat_FailsValidation(string mobile)
        {
            var cmd = ValidCommand();
            cmd.MobileNumber = mobile;
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync(mobile, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        [Fact]
        public async Task MobileNumber_AlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        // ── ContactTypeId Rules ───────────────────────────────────────────────

        [Fact]
        public async Task ContactTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.ContactTypeId = 0;
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ContactTypeId);
        }

        [Fact]
        public async Task ContactTypeId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MobileAlreadyExistsAsync("9876543210", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ContactTypeExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ContactTypeId);
        }

        // ── PartyId (optional) Rules ──────────────────────────────────────────

        [Fact]
        public async Task PartyId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.PartyId = 999;
            SetupAllAsyncMocks();
            _mockPartyLookup.Setup(p => p.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        // ── Email Rules ───────────────────────────────────────────────────────

        [Fact]
        public async Task Email_InvalidFormat_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Email = "not-an-email";
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Email_ValidFormat_PassesValidation()
        {
            var cmd = ValidCommand();
            cmd.Email = "test@example.com";
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
    }
}
