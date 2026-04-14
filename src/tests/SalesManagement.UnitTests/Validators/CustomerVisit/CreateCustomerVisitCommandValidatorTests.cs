using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Presentation.Validation.CustomerVisit;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.CustomerVisit
{
    public sealed class CreateCustomerVisitCommandValidatorTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CreateCustomerVisitCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

        private void SetupAllAsyncMocks(int customerId = 1, int visitTypeId = 1, int marketingOfficerId = 1)
        {
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(customerId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(visitTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(marketingOfficerId)).ReturnsAsync(true);
            _mockAccessFilter.Setup(f => f.CanAccessCustomerAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private static CreateCustomerVisitCommand ValidCommand() => new()
        {
            CustomerId = 1,
            VisitTypeId = 1,
            VisitDateTime = DateTimeOffset.Now.AddHours(-1),
            MarketingOfficerId = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── CustomerId Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task CustomerId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.CustomerId = 0;
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        [Fact]
        public async Task CustomerId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        // ── VisitTypeId Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task VisitTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.VisitTypeId = 0;
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.VisitTypeId);
        }

        [Fact]
        public async Task VisitTypeId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.VisitTypeId);
        }

        // ── MarketingOfficerId Rules ───────────────────────────────────────────

        [Fact]
        public async Task MarketingOfficerId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.MarketingOfficerId = 0;
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        // ── VisitDateTime Rules ───────────────────────────────────────────────

        [Fact]
        public async Task VisitDateTime_Default_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.VisitDateTime = default;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.VisitDateTime);
        }

        // ── Latitude Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(91.0)]
        [InlineData(-91.0)]
        public async Task Latitude_OutOfRange_FailsValidation(double lat)
        {
            var cmd = ValidCommand();
            cmd.Latitude = (decimal)lat;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Latitude);
        }

        // ── Longitude Rules ───────────────────────────────────────────────────

        [Theory]
        [InlineData(181.0)]
        [InlineData(-181.0)]
        public async Task Longitude_OutOfRange_FailsValidation(double lon)
        {
            var cmd = ValidCommand();
            cmd.Longitude = (decimal)lon;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Longitude);
        }

        // ── Product Detail Rules ──────────────────────────────────────────────

        [Fact]
        public async Task Products_WithDuplicateItemIds_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Products = new List<CreateCustomerVisitProductDto>
            {
                new() { ItemId = 5 },
                new() { ItemId = 5 }
            };
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(5)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Duplicate product"));
        }

        [Fact]
        public async Task Products_WithInvalidItemId_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Products = new List<CreateCustomerVisitProductDto>
            {
                new() { ItemId = 999 }
            };
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
