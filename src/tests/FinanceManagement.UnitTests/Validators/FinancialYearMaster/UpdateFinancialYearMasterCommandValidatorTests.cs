using Contracts.Interfaces;
using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.Presentation.Validation.FinancialYearMaster;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.FinancialYearMaster
{
    public sealed class UpdateFinancialYearMasterCommandValidatorTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateFinancialYearMasterCommandValidator CreateValidator()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            return new UpdateFinancialYearMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            var result = await CreateValidator().TestValidateAsync(FinancialYearMasterBuilders.ValidUpdateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var validator = new UpdateFinancialYearMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);

            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(id: 99);
            var result = await validator.TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_IsActiveOutOfRange_FailsValidation(int isActive)
        {
            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(isActive: isActive);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData("FY24")]
        [InlineData("2024")]
        public async Task Validate_InvalidCodeFormat_FailsValidation(string code)
        {
            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(code: code);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FinancialYearCode);
        }
    }
}
