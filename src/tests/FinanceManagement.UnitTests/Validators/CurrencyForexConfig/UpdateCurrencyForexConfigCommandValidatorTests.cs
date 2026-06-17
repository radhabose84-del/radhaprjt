using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;
using FinanceManagement.Presentation.Validation.CurrencyForexConfig;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.CurrencyForexConfig
{
    public sealed class UpdateCurrencyForexConfigCommandValidatorTests
    {
        private readonly Mock<ICurrencyForexConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateCurrencyForexConfigCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        private static UpdateCurrencyForexConfigCommand ValidCommand() =>
            new() { Id = 1, CurrencyTypeName = "Forex", IsActive = 1 };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_DuplicateName_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync("Forex", 1, 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.CurrencyTypeName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_InvalidIsActive_Fails(int isActive)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.IsActive = isActive;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
