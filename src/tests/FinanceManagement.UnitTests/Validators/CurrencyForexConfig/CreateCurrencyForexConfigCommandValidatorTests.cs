using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;
using FinanceManagement.Presentation.Validation.CurrencyForexConfig;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.CurrencyForexConfig
{
    public sealed class CreateCurrencyForexConfigCommandValidatorTests
    {
        private readonly Mock<ICurrencyForexConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateCurrencyForexConfigCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        private static CreateCurrencyForexConfigCommand ValidCommand() =>
            new() { CurrencyTypeCode = "FOREX", CurrencyTypeName = "Forex" };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_Fails(string? code)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.CurrencyTypeCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.CurrencyTypeName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeName);
        }

        [Theory]
        [InlineData("FX-01")]   // hyphen
        [InlineData("FX 01")]   // space
        [InlineData("FX@01")]   // special char
        public async Task Validate_NonAlphanumericCode_Fails(string code)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.CurrencyTypeCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("FOREX", 1, It.IsAny<int?>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateName_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync("Forex", 1, It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeName);
        }
    }
}
