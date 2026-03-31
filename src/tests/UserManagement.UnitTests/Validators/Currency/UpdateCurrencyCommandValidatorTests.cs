using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Currency;

namespace UserManagement.UnitTests.Validators.Currency
{
    public sealed class UpdateCurrencyCommandValidatorTests
    {
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private UpdateCurrencyCommandValidator CreateValidator()
        {
            _ip.Setup(x => x.GetGroupCode()).Returns("ADMIN");
            _ip.Setup(x => x.GetCompanyId()).Returns(1);
            _ip.Setup(x => x.GetUnitId()).Returns(1);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var db = new ApplicationDbContext(options, _ip.Object, tz.Object);
            var maxLenProvider = new MaxLengthProvider(db);

            return new UpdateCurrencyCommandValidator(maxLenProvider);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateCurrencyCommand { Id = 1, Name = "USDollar", IsActive = 1 };
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new UpdateCurrencyCommand { Id = 1, Name = name, IsActive = 1 };
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_NameExceedsMaxLength_FailsValidation()
        {
            // MaxLength for Name from EF metadata fallback is 50
            var command = new UpdateCurrencyCommand
            {
                Id = 1,
                Name = new string('A', 51),
                IsActive = 1
            };
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Theory]
        [InlineData("Dollar 123")]
        [InlineData("US$")]
        public async Task Validate_NonAlphabeticName_FailsValidation(string name)
        {
            var command = new UpdateCurrencyCommand { Id = 1, Name = name, IsActive = 1 };
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}
