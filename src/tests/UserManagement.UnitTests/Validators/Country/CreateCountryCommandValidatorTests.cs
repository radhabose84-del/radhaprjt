using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Country;
using UserManagement.UnitTests.TestData;
using Contracts.Interfaces;

namespace UserManagement.UnitTests.Validators.Country
{
    public sealed class CreateCountryCommandValidatorTests
    {
        private readonly Mock<ICountryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            mockIp.Setup(x => x.GetUserName()).Returns("test");
            mockIp.Setup(x => x.GetUserId()).Returns(1);
            mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            mockIp.Setup(x => x.GetUnitId()).Returns(1);
            mockIp.Setup(x => x.GetGroupCode()).Returns("ADMIN");

            var mockTz = new Mock<UserManagement.Application.Common.Interfaces.ITimeZoneService>(MockBehavior.Loose);
            mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CreateCountryValidatorDb_{Guid.NewGuid()}")
                .Options;

            var dbContext = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            dbContext.Database.EnsureCreated();
            return new MaxLengthProvider(dbContext);
        }

        private CreateCountryCommandValidator CreateValidator()
        {
            return new CreateCountryCommandValidator(
                CreateMaxLengthProvider(),
                _mockCommandRepo.Object);
        }

        private void SetupNoDuplicates()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCountryName_FailsValidation(string? countryName)
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryName: countryName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCountryCode_FailsValidation(string? countryCode)
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryCode: countryCode!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
        }

        [Fact]
        public async Task Validate_CountryCodeExceedsMaxLength_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryCode: "ABCDEF");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
        }

        [Fact]
        public async Task Validate_CountryNameExceedsMaxLength_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryName: new string('A', 51));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryName);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = CountryBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = CountryBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
        }

        [Fact]
        public async Task Validate_WhitespaceOnlyCountryName_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryName: "   ");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryName);
        }

        [Fact]
        public async Task Validate_SpecialCharInCode_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidCreateCommand(countryCode: "I@D");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
        }
    }
}
