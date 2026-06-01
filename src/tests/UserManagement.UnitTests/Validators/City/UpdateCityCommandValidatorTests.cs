using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Validation.Common;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.City;
using UserManagement.Presentation.Validation.Country;
using UserManagement.UnitTests.TestData;
using Contracts.Interfaces;

namespace UserManagement.UnitTests.Validators.City
{
    public sealed class UpdateCityCommandValidatorTests
    {
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
                .UseInMemoryDatabase(databaseName: $"UpdateCityValidatorDb_{Guid.NewGuid()}")
                .Options;

            var dbContext = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            dbContext.Database.EnsureCreated();
            return new MaxLengthProvider(dbContext);
        }

        private static UpdateCityCommandValidator CreateValidator()
        {
            return new UpdateCityCommandValidator(CreateMaxLengthProvider());
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = CityBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCityName_FailsValidation(string? cityName)
        {
            var command = CityBuilders.ValidUpdateCommand(cityName: cityName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCityCode_FailsValidation(string? cityCode)
        {
            var command = CityBuilders.ValidUpdateCommand(cityCode: cityCode!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityCode);
        }

        [Fact]
        public async Task Validate_CityCodeExceedsMaxLength_FailsValidation()
        {
            var command = CityBuilders.ValidUpdateCommand(cityCode: "ABCDEF");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityCode);
        }

        [Fact]
        public async Task Validate_CityNameExceedsMaxLength_FailsValidation()
        {
            var command = CityBuilders.ValidUpdateCommand(cityName: new string('A', 51));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityName);
        }
    }
}
