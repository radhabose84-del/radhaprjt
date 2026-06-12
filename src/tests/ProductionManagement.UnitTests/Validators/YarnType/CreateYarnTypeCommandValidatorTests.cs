using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Presentation.Validation.YarnType;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.YarnType
{
    public sealed class CreateYarnTypeCommandValidatorTests
    {
        private readonly Mock<IYarnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateYarnTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateYarnTypeCommand());
            result.ShouldHaveAnyValidationError();
        }

        private static CreateYarnTypeCommand ValidBase() =>
            new() { YarnTypeCode = "YT001", YarnTypeName = "Cotton" };

        [Fact]
        public async Task Validate_NegativeAdditionalPrice_FailsValidation()
        {
            var command = ValidBase();
            command.AdditionalPrice = -1m;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AdditionalPrice);
        }

        [Fact]
        public async Task Validate_AdditionalPriceWithoutCurrency_FailsValidation()
        {
            var command = ValidBase();
            command.AdditionalPrice = 100m;
            command.CurrencyId = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId);
        }

        [Fact]
        public async Task Validate_NonExistentCurrency_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = ValidBase();
            command.AdditionalPrice = 100m;
            command.CurrencyId = 999;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId);
        }

        [Fact]
        public async Task Validate_ValidPriceAndCurrency_PassesPriceAndCurrencyRules()
        {
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = ValidBase();
            command.AdditionalPrice = 100m;
            command.CurrencyId = 1;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.AdditionalPrice);
            result.ShouldNotHaveValidationErrorFor(x => x.CurrencyId);
        }
    }
}
