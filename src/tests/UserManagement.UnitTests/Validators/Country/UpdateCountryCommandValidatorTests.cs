using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Country;
using UserManagement.UnitTests.TestData;
using Contracts.Interfaces;

namespace UserManagement.UnitTests.Validators.Country
{
    public sealed class UpdateCountryCommandValidatorTests
    {
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
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
                .UseInMemoryDatabase(databaseName: $"UpdateCountryValidatorDb_{Guid.NewGuid()}")
                .Options;

            var dbContext = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            dbContext.Database.EnsureCreated();
            return new MaxLengthProvider(dbContext);
        }

        private UpdateCountryCommandValidator CreateValidator()
        {
            return new UpdateCountryCommandValidator(
                _mockQueryRepo.Object,
                _mockCommandRepo.Object,
                CreateMaxLengthProvider());
        }

        private void SetupNoDuplicates()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCountryName_FailsValidation(string? countryName)
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand(countryName: countryName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCountryCode_FailsValidation(string? countryCode)
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand(countryCode: countryCode!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
        }

        [Fact]
        public async Task Validate_IdZero_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand(id: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_IsActiveInvalid_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand(isActive: 5);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_CountryCodeExceedsMaxLength_FailsValidation()
        {
            SetupNoDuplicates();
            var command = CountryBuilders.ValidUpdateCommand(countryCode: "ABCDEF");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = CountryBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
        }
    }
}
