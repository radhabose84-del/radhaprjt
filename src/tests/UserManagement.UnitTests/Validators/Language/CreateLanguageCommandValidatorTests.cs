using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Languages;
using UserManagement.Infrastructure.Data;
using UserManagement.UnitTests.TestData;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.UnitTests.Validators.Language
{
    public sealed class CreateLanguageCommandValidatorTests
    {
        private CreateLanguageCommandValidator CreateValidator()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetGroupCode()).Returns("ADMIN");
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
            var maxLenProvider = new MaxLengthProvider(db);

            return new CreateLanguageCommandValidator(maxLenProvider);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            var command = LanguageBuilders.ValidCreateCommand();
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            var command = LanguageBuilders.ValidCreateCommand(name: name!);
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_Fails(string? code)
        {
            var command = LanguageBuilders.ValidCreateCommand(code: code!);
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_NameExceedsMaxLength_Fails()
        {
            var command = LanguageBuilders.ValidCreateCommand(
                name: new string('A', 100));
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_Fails()
        {
            var command = LanguageBuilders.ValidCreateCommand(
                code: new string('A', 50));
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }
    }
}
