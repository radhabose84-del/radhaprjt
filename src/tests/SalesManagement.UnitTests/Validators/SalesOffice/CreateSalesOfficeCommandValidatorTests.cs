using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Presentation.Validation.SalesOffice;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOffice
{
    public class CreateSalesOfficeCommandValidatorTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesOfficeCommandValidator CreateValidator()
            => new CreateSalesOfficeCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllAsyncMocks(
            string name = "Test Sales Office",
            int salesOrganisationId = 1,
            int cityId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(name, salesOrganisationId, null))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(salesOrganisationId))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(cityId))
                .ReturnsAsync(true);
        }

        private void SetupFKMocks(int salesOrganisationId = 1, int cityId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(salesOrganisationId))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(cityId))
                .ReturnsAsync(true);
        }

        private void SetupAlreadyExistsAny(bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
                .ReturnsAsync(exists);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            // Name must be alphanumeric (no spaces) to pass Alphanumeric rule
            var command = SalesOfficeBuilders.ValidCreateCommand(name: "TestSalesOffice");
            SetupAllAsyncMocks(name: "TestSalesOffice");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SalesOfficeName Rules ─────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesOfficeName_Empty_FailsValidation(string? name)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(name: name!);
            SetupFKMocks();
            // AlreadyExists .When guard skips when name is empty

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Fact]
        public async Task SalesOfficeName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = SalesOfficeBuilders.ValidCreateCommand(name: longName);
            SetupFKMocks();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Fact]
        public async Task SalesOfficeName_MaxLength100_PassesValidation()
        {
            var maxName = new string('A', 100);
            var command = SalesOfficeBuilders.ValidCreateCommand(name: maxName);
            SetupAllAsyncMocks(name: maxName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Theory]
        [InlineData("OFF-01")]   // hyphen
        [InlineData("OFF 01")]   // space
        [InlineData("OFF@01")]   // special char
        public async Task SalesOfficeName_NotAlphanumeric_FailsValidation(string name)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(name: name);
            SetupFKMocks();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Fact]
        public async Task SalesOfficeName_AlreadyExists_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(name: "DuplicateOffice");
            SetupFKMocks();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("DuplicateOffice", 1, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists."));
        }

        // ── SalesOrganisationId Rules ─────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesOrganisationId_ZeroOrNegative_FailsValidation(int salesOrgId)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(salesOrganisationId: salesOrgId);
            // FK .When guard skips when <= 0; AlreadyExists .When also skips
            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId);
        }

        [Fact]
        public async Task SalesOrganisationId_NotFound_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(salesOrganisationId: 999);
            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(999))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(1))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), 999, null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId);
        }

        // ── CityId Rules ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CityId_ZeroOrNegative_FailsValidation(int cityId)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(cityId: cityId);
            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(1))
                .ReturnsAsync(true);
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId);
        }

        [Fact]
        public async Task CityId_NotFound_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(cityId: 999);
            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(1))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(999))
                .ReturnsAsync(false);
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CityId);
        }

        // ── Pincode Rules ─────────────────────────────────────────────────────

        [Theory]
        [InlineData("12345")]     // 5 digits
        [InlineData("1234567")]   // 7 digits
        [InlineData("ABCDEF")]    // letters
        public async Task Pincode_InvalidFormat_FailsValidation(string pincode)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(pincode: pincode);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Pincode);
        }

        [Fact]
        public async Task Pincode_Valid6Digit_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(pincode: "560001");
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Pincode);
        }

        [Fact]
        public async Task Pincode_Empty_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(pincode: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Pincode);
        }

        // ── Phone Rules ───────────────────────────────────────────────────────

        [Theory]
        [InlineData("12345")]         // too short
        [InlineData("ABCDEFGHIJ")]    // letters
        public async Task Phone_InvalidFormat_FailsValidation(string phone)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(phone: phone);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Theory]
        [InlineData("+911234567890")]  // with country code
        [InlineData("1234567890")]     // plain 10 digits
        public async Task Phone_ValidFormat_PassesValidation(string phone)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(phone: phone);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public async Task Phone_Empty_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(phone: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Phone);
        }

        // ── Email Rules ───────────────────────────────────────────────────────

        [Theory]
        [InlineData("notanemail")]
        [InlineData("user@")]
        [InlineData("@nodomain")]
        public async Task Email_InvalidFormat_FailsValidation(string email)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(email: email);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("user@gmail.com")]
        [InlineData("user@bannarimills.co.in")]
        public async Task Email_ValidFormat_PassesValidation(string email)
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(email: email);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Email_Empty_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(email: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        // ── Optional MaxLength Fields ─────────────────────────────────────────

        [Fact]
        public async Task ResponsibleManager_TooLong_FailsValidation()
        {
            var longManager = new string('A', 101);
            var command = SalesOfficeBuilders.ValidCreateCommand(responsibleManager: longManager);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleManager);
        }

        [Fact]
        public async Task RegionTerritory_TooLong_FailsValidation()
        {
            var longRegion = new string('A', 101);
            var command = SalesOfficeBuilders.ValidCreateCommand(regionTerritory: longRegion);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RegionTerritory);
        }

        [Fact]
        public async Task Address_TooLong_FailsValidation()
        {
            var longAddress = new string('A', 501);
            var command = SalesOfficeBuilders.ValidCreateCommand(address: longAddress);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Address);
        }

        [Fact]
        public async Task Address_Empty_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand(address: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Address);
        }
    }
}
