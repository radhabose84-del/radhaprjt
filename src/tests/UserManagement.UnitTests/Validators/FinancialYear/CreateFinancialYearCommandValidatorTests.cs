using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.FinancialYear;

namespace UserManagement.UnitTests.Validators.FinancialYear
{
    public sealed class CreateFinancialYearCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FinancialYearDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static CreateFinancialYearCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static CreateFinancialYearCommand ValidCommand(int year = 2024) =>
            new CreateFinancialYearCommand
            {
                StartYear = year.ToString(),
                StartDate = new DateTime(year, 4, 1),
                EndDate = new DateTime(year + 1, 3, 31),
                FinYearName = $"FY {year}-{year + 1}"
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyStartYear_FailsValidation(string? startYear)
        {
            var command = ValidCommand();
            command.StartYear = startYear;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.StartYear);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFinYearName_FailsValidation(string? finYearName)
        {
            var command = ValidCommand();
            command.FinYearName = finYearName;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.FinYearName);
        }

        [Fact]
        public async Task Validate_InvalidStartYear_NotFourDigits_FailsValidation()
        {
            var command = ValidCommand();
            command.StartYear = "24";  // not 4 digits
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.StartYear);
        }

        [Fact]
        public async Task Validate_StartDateNotApril1st_FailsValidation()
        {
            var command = ValidCommand();
            command.StartDate = new DateTime(2024, 3, 1); // Not April 1st
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Fact]
        public async Task Validate_EndDateNotMarch31st_FailsValidation()
        {
            var command = ValidCommand();
            command.EndDate = new DateTime(2025, 4, 1); // Not March 31st
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }
    }
}
