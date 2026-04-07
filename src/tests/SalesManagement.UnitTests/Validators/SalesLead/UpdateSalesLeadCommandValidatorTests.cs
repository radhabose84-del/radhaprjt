using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.UpdateSalesLead;
using SalesManagement.Presentation.Validation.SalesLead;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesLead
{
    public sealed class UpdateSalesLeadCommandValidatorTests
    {
        private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesLeadCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateSalesLeadCommand ValidCommand() => new()
        {
            Id = 1,
            ContactName = "John Doe",
            MobileNumber = "9876543210",
            MarketingOfficerId = 1,
            InteractionDate = DateTimeOffset.UtcNow,
            IsActive = 1
        };

        private void SetupAllAsyncMocks(int id = 1, string mobile = "9876543210", int officerId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync(mobile!, It.IsAny<int?>())).ReturnsAsync(false);
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
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
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
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task MarketingOfficerNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsForProspectAsync("9876543210", It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MobileNumberExistsInSalesContactAsync("9876543210")).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }
    }
}
