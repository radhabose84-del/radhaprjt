using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Presentation.Validation.CustomerVisit;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.CustomerVisit
{
    public sealed class UpdateCustomerVisitCommandValidatorTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateCustomerVisitCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int customerId = 1, int visitTypeId = 1, int marketingOfficerId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(customerId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(visitTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(marketingOfficerId)).ReturnsAsync(true);
        }

        private static UpdateCustomerVisitCommand ValidCommand() => new()
        {
            Id = 1,
            CustomerId = 1,
            VisitTypeId = 1,
            VisitDateTime = DateTimeOffset.Now.AddHours(-1),
            MarketingOfficerId = 1,
            IsActive = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── CustomerId Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task CustomerId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VisitTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        // ── Latitude / Longitude Rules ────────────────────────────────────────

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
    }
}
