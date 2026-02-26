using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using SalesManagement.Presentation.Validation.MarketingOfficer;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MarketingOfficer
{
    public class UpdateMarketingOfficerCommandValidatorTests
    {
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMarketingOfficerCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        /// <summary>
        /// Sets up all async mocks to pass (valid state).
        /// Specific tests override individual mocks as needed.
        /// </summary>
        private void SetupAllAsyncMocksPass()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SalesOfficeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupsAllExistAsync(It.IsAny<List<int>>())).ReturnsAsync(true);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand();
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotFound Rules ───────────────────────────────────────────────

        [Fact]
        public async Task Id_Zero_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(id: 0);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(id: 99);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── EmployeeName Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmployeeName_Empty_FailsValidation(string? name)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(employeeName: name!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeName);
        }

        // ── Unit / Department / Designation Rules ─────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Unit_Empty_FailsValidation(string? unit)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(unit: unit!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Unit);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Department_Empty_FailsValidation(string? department)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(department: department!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Department);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Designation_Empty_FailsValidation(string? designation)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(designation: designation!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Designation);
        }

        // ── IsActive ByteValue Rule ───────────────────────────────────────────

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(99)]
        public async Task IsActive_OutOfRange_FailsValidation(int isActive)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        // ── SalesOfficeId FK Rules ────────────────────────────────────────────

        [Fact]
        public async Task SalesOfficeId_DoesNotExist_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(salesOfficeId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.SalesOfficeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        // ── MobileNo Custom Rules ─────────────────────────────────────────────

        [Theory]
        [InlineData("12345")]       // too short
        [InlineData("5876543210")]  // starts with 5
        [InlineData("ABCDEFGHIJ")]  // letters
        public async Task MobileNo_InvalidFormat_FailsValidation(string mobile)
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(mobileNo: mobile);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MobileNo)
                  .WithErrorMessage("MobileNo must be a valid 10-digit mobile number starting with 6-9.");
        }

        [Fact]
        public async Task MobileNo_Null_SkipsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(mobileNo: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MobileNo);
        }

        // ── Email Custom Rules ────────────────────────────────────────────────

        [Fact]
        public async Task Email_InvalidFormat_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(email: "not-an-email");
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email must be a valid email address.");
        }

        [Fact]
        public async Task Email_Null_SkipsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(email: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        // ── SalesGroups Custom Rules ──────────────────────────────────────────

        [Fact]
        public async Task SalesGroups_Null_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand();
            command.SalesGroups = null!;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("SalesGroups is required.");
        }

        [Fact]
        public async Task SalesGroups_Empty_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(
                salesGroups: new List<UpdateOfficerSalesGroupDto>());
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("At least one Sales Group is required.");
        }

        [Fact]
        public async Task SalesGroups_DuplicateIds_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(
                salesGroups: new List<UpdateOfficerSalesGroupDto>
                {
                    new() { SalesGroupId = 1 },
                    new() { SalesGroupId = 1 }
                });
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("Duplicate SalesGroupId values are not allowed.");
        }

        [Fact]
        public async Task SalesGroups_InvalidIds_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidUpdateCommand(
                salesGroups: new List<UpdateOfficerSalesGroupDto>
                {
                    new() { SalesGroupId = 99 }
                });
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.SalesGroupsAllExistAsync(It.IsAny<List<int>>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("One or more SalesGroupId values are inactive or deleted.");
        }
    }
}
