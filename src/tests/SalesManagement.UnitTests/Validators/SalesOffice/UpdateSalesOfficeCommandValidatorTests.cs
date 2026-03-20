using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using SalesManagement.Presentation.Validation.SalesOffice;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOffice
{
    public class UpdateSalesOfficeCommandValidatorTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesOfficeCommandValidator CreateValidator()
            => new UpdateSalesOfficeCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllAsyncMocks(
            int id = 1,
            string name = "Updated Sales Office",
            int salesOrganisationId = 1,
            int cityId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(salesOrganisationId))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(cityId))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(name, salesOrganisationId, id))
                .ReturnsAsync(false);
        }

        private void SetupIdExists(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
        }

        private void SetupIdNotFound(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
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
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(exists);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            // Name must be alphanumeric (no spaces) to pass Alphanumeric rule
            var command = SalesOfficeBuilders.ValidUpdateCommand(name: "UpdatedSalesOffice");
            SetupAllAsyncMocks(name: "UpdatedSalesOffice");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);

            var command = SalesOfficeBuilders.ValidUpdateCommand(id: id);
            SetupFKMocks();
            // AlreadyExists .When guard requires Id > 0 — skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(id: 99);
            SetupIdNotFound(99);
            SetupFKMocks();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── SalesOfficeName Rules ─────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesOfficeName_Empty_FailsValidation(string? name)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(name: name!);
            SetupIdExists();
            SetupFKMocks();
            // AlreadyExists .When guard skips when name is empty

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Fact]
        public async Task SalesOfficeName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = SalesOfficeBuilders.ValidUpdateCommand(name: longName);
            SetupIdExists();
            SetupFKMocks();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Theory]
        [InlineData("OFF-01")]   // hyphen
        [InlineData("OFF 01")]   // space
        [InlineData("OFF@01")]   // special char
        public async Task SalesOfficeName_NotAlphanumeric_FailsValidation(string name)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(name: name);
            SetupIdExists();
            SetupFKMocks();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeName);
        }

        [Fact]
        public async Task SalesOfficeName_AlreadyExists_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(name: "DuplicateOffice");
            SetupIdExists();
            SetupFKMocks();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("DuplicateOffice", 1, 1))
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
            var command = SalesOfficeBuilders.ValidUpdateCommand(salesOrganisationId: salesOrgId);
            SetupIdExists();
            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId);
        }

        [Fact]
        public async Task SalesOrganisationId_NotFound_FailsValidation()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(salesOrganisationId: 999);
            SetupIdExists();
            _mockQueryRepo
                .Setup(r => r.SalesOrganisationExistsAsync(999))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.CityExistsAsync(1))
                .ReturnsAsync(true);
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId);
        }

        // ── CityId Rules ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CityId_ZeroOrNegative_FailsValidation(int cityId)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(cityId: cityId);
            SetupIdExists();
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
            var command = SalesOfficeBuilders.ValidUpdateCommand(cityId: 999);
            SetupIdExists();
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
            var command = SalesOfficeBuilders.ValidUpdateCommand(pincode: pincode);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Pincode);
        }

        [Fact]
        public async Task Pincode_Valid6Digit_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(pincode: "560002");
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Pincode);
        }

        [Fact]
        public async Task Pincode_Empty_PassesValidation()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(pincode: null!);
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
            var command = SalesOfficeBuilders.ValidUpdateCommand(phone: phone);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Theory]
        [InlineData("+911234567891")]  // with country code
        [InlineData("1234567891")]     // plain 10 digits
        public async Task Phone_ValidFormat_PassesValidation(string phone)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(phone: phone);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Phone);
        }

        // ── Email Rules ───────────────────────────────────────────────────────

        [Theory]
        [InlineData("notanemail")]
        [InlineData("user@invalid.com")]
        public async Task Email_InvalidFormat_FailsValidation(string email)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(email: email);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("user@gmail.com")]
        [InlineData("user@bannarimills.co.in")]
        public async Task Email_ValidFormat_PassesValidation(string email)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(email: email);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        // ── Optional MaxLength Fields ─────────────────────────────────────────

        [Fact]
        public async Task ResponsibleManager_TooLong_FailsValidation()
        {
            var longManager = new string('A', 101);
            var command = SalesOfficeBuilders.ValidUpdateCommand(responsibleManager: longManager);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleManager);
        }

        [Fact]
        public async Task RegionTerritory_TooLong_FailsValidation()
        {
            var longRegion = new string('A', 101);
            var command = SalesOfficeBuilders.ValidUpdateCommand(regionTerritory: longRegion);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RegionTerritory);
        }

        [Fact]
        public async Task Address_TooLong_FailsValidation()
        {
            var longAddress = new string('A', 501);
            var command = SalesOfficeBuilders.ValidUpdateCommand(address: longAddress);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Address);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
