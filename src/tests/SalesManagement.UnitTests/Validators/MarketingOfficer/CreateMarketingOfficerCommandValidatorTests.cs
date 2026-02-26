using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Presentation.Validation.MarketingOfficer;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.MarketingOfficer
{
    public class CreateMarketingOfficerCommandValidatorTests
    {
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMarketingOfficerCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        /// <summary>
        /// Sets up all async mocks to pass (valid state).
        /// Specific tests override individual mocks as needed.
        /// </summary>
        private void SetupAllAsyncMocksPass()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SalesOfficeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupsAllExistAsync(It.IsAny<List<int>>())).ReturnsAsync(true);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── EmployeeNo Rules ──────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmployeeNo_Empty_FailsValidation(string? code)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(employeeNo: code!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeNo);
        }

        [Theory]
        [InlineData("EMP-01")]   // hyphen
        [InlineData("EMP 01")]   // space
        [InlineData("EMP@01")]   // special char
        public async Task EmployeeNo_NonAlphanumeric_FailsValidation(string code)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(employeeNo: code);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeNo);
        }

        [Fact]
        public async Task EmployeeNo_AlreadyExists_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(employeeNo: "EXIST001");
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeNo);
        }

        // ── EmployeeName Rules ────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmployeeName_Empty_FailsValidation(string? name)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(employeeName: name!);
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
            var command = MarketingOfficerBuilders.ValidCreateCommand(unit: unit!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Unit);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Department_Empty_FailsValidation(string? department)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(department: department!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Department);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Designation_Empty_FailsValidation(string? designation)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(designation: designation!);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Designation);
        }

        // ── SalesOfficeId FK Rules ────────────────────────────────────────────

        [Fact]
        public async Task SalesOfficeId_DoesNotExist_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(salesOfficeId: 999);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.SalesOfficeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        // ── MobileNo Custom Rules ─────────────────────────────────────────────

        [Theory]
        [InlineData("12345")]       // too short
        [InlineData("12345678901")] // too long
        [InlineData("5876543210")]  // starts with 5
        [InlineData("ABCDEFGHIJ")]  // letters
        public async Task MobileNo_InvalidFormat_FailsValidation(string mobile)
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(mobileNo: mobile);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MobileNo)
                  .WithErrorMessage("MobileNo must be a valid 10-digit mobile number starting with 6-9.");
        }

        [Fact]
        public async Task MobileNo_Null_SkipsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(mobileNo: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MobileNo);
        }

        // ── Email Custom Rules ────────────────────────────────────────────────

        [Fact]
        public async Task Email_InvalidFormat_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(email: "not-an-email");
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email must be a valid email address.");
        }

        [Fact]
        public async Task Email_Null_SkipsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(email: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        // ── SalesGroups Custom Rules ──────────────────────────────────────────

        [Fact]
        public async Task SalesGroups_Null_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            command.SalesGroups = null!;
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("SalesGroups is required.");
        }

        [Fact]
        public async Task SalesGroups_Empty_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(
                salesGroups: new List<CreateOfficerSalesGroupDto>());
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroups)
                  .WithErrorMessage("At least one Sales Group is required.");
        }

        [Fact]
        public async Task SalesGroups_DuplicateIds_FailsValidation()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(
                salesGroups: new List<CreateOfficerSalesGroupDto>
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
            var command = MarketingOfficerBuilders.ValidCreateCommand(
                salesGroups: new List<CreateOfficerSalesGroupDto>
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
