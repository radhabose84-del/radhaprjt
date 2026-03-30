using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Divisions;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.Division
{
    public sealed class UpdateDivisionCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"DivisionUpdateValidatorDb_{Guid.NewGuid()}")
                .Options;

            var mockIpService = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            mockIpService.Setup(s => s.GetUserId()).Returns(1);

            var mockTimeZone = new Mock<ITimeZoneService>(MockBehavior.Loose);
            mockTimeZone.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var dbContext = new ApplicationDbContext(options, mockIpService.Object, mockTimeZone.Object);
            return new MaxLengthProvider(dbContext);
        }

        private static UpdateDivisionCommandValidator CreateValidator()
        {
            var maxLengthProvider = CreateMaxLengthProvider();
            return new UpdateDivisionCommandValidator(maxLengthProvider);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = DivisionBuilders.ValidUpdateCommand(name: name!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyShortName_FailsValidation(string? shortName)
        {
            var command = DivisionBuilders.ValidUpdateCommand(shortName: shortName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_NameExceedsMaxLength_FailsValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand(name: new string('A', 101));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_ShortNameExceedsMaxLength_FailsValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand(shortName: new string('A', 51));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_ZeroCompanyId_FailsValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand(companyId: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Validate_NegativeCompanyId_FailsValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand(companyId: -1);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Validate_ValidCompanyId_PassesValidation()
        {
            var command = DivisionBuilders.ValidUpdateCommand(companyId: 1);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.CompanyId);
        }
    }
}
