using FluentValidation.TestHelper;
using Shared.Validation.Common;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Presentation.Validation.City;
using UserManagement.Presentation.Validation.Common;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.City
{
    public sealed class CreateCityCommandValidatorTests
    {
        private static Mock<IMaxLengthProvider> CreateMockMaxLengthProvider()
        {
            var mock = new Mock<IMaxLengthProvider>(MockBehavior.Loose);
            mock.Setup(m => m.GetMaxLength<UserManagement.Domain.Entities.Cities>("CityCode"))
                .Returns(5);
            mock.Setup(m => m.GetMaxLength<UserManagement.Domain.Entities.Cities>("CityName"))
                .Returns(50);
            return mock;
        }

        private static Mock<IValidationRuleProvider> CreateMockRuleProvider()
        {
            var rules = new List<ValidationRule>
            {
                new ValidationRule { Rule = "NotEmpty", Error = "is required." },
                new ValidationRule { Rule = "MaxLength", Error = " cannot be longer than  " }
            };
            var mock = new Mock<IValidationRuleProvider>(MockBehavior.Strict);
            mock.Setup(m => m.GetRules()).Returns(rules);
            return mock;
        }

        private CreateCityCommandValidator CreateValidator() =>
            new(CreateMockMaxLengthProvider().Object, CreateMockRuleProvider().Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = CityBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCityName_FailsValidation(string? cityName)
        {
            var command = CityBuilders.ValidCreateCommand(cityName: cityName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCityCode_FailsValidation(string? cityCode)
        {
            var command = CityBuilders.ValidCreateCommand(cityCode: cityCode!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityCode);
        }

        [Fact]
        public async Task Validate_CityCodeExceedsMaxLength_FailsValidation()
        {
            var command = CityBuilders.ValidCreateCommand(cityCode: "ABCDEF"); // 6 chars > 5 max
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityCode);
        }

        [Fact]
        public async Task Validate_CityNameExceedsMaxLength_FailsValidation()
        {
            var command = CityBuilders.ValidCreateCommand(cityName: new string('A', 51)); // 51 chars > 50 max
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CityName);
        }

        [Fact]
        public async Task Validate_CityCodeWithinMaxLength_PassesValidation()
        {
            var command = CityBuilders.ValidCreateCommand(cityCode: "CTY01"); // 5 chars = 5 max
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.CityCode);
        }
    }
}
