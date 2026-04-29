using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Presentation.Validation.SalesLead;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesLead
{
    public sealed class CreateSalesLeadCommandValidatorTests
    {
        private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CreateSalesLeadCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

        private static CreateSalesLeadCommand ValidCommand() => new()
        {
            ContactName = "John Doe",
            MobileNumber = "9876543210",
            MarketingOfficerId = 1,
            InteractionDate = DateTimeOffset.UtcNow
        };

        private void SetupAllAsyncMocks(string mobile = "9876543210", int officerId = 1)
        {
            // PartyId is null → MobileNumberExistsForProspect fires
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync(mobile!, It.IsAny<int?>())).ReturnsAsync(false);
            // ContactId is null → MobileNumberExistsInSalesContact fires
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync(mobile!)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(officerId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ContactName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.ContactName = name;
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
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
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        [Fact]
        public async Task MarketingOfficerId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.MarketingOfficerId = 0;
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        [Fact]
        public async Task DuplicateMobileForProspect_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
        }

        [Fact]
        public async Task MarketingOfficerNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        [Fact]
        public async Task UomId_Null_PassesValidation()
        {
            var cmd = ValidCommand();
            cmd.UomId = null;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveValidationErrorFor(x => x.UomId);
        }

        [Fact]
        public async Task UomId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.UomId = 999;
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.UomExistsAsync(999)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.UomId);
        }
    }
}
