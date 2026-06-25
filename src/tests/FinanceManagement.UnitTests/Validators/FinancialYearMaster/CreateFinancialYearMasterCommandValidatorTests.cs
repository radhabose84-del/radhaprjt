using Contracts.Interfaces;
using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Presentation.Validation.FinancialYearMaster;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.FinancialYearMaster
{
    public sealed class CreateFinancialYearMasterCommandValidatorTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateFinancialYearMasterCommandValidator CreateValidator()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.OverlapsExistingRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            return new CreateFinancialYearMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand();
            cmd.FinancialYearCode = code;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FinancialYearCode);
        }

        [Theory]
        [InlineData("FY24")]               // not numeric format
        [InlineData("2024")]               // missing suffix
        [InlineData("2024-2025")]          // 4-digit suffix
        [InlineData("24-25")]              // 2-digit prefix
        public async Task Validate_InvalidCodeFormat_FailsValidation(string code)
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand(code: code);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FinancialYearCode)
                  .WithErrorMessage("Financial Year Code must follow format YYYY-YY (e.g. 2024-25).");
        }

        [Fact]
        public async Task Validate_CodeSuffixDoesntMatchStartYear_FailsValidation()
        {
            // 2024-04-01 start, but code "2024-26" — suffix should be "25"
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand(code: "2024-26");
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("match Start Date year"));
        }

        [Fact]
        public async Task Validate_StartDateNotFirstOfMonth_FailsValidation()
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand(startDate: new DateOnly(2024, 4, 15));
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("1st of a month"));
        }

        [Fact]
        public async Task Validate_NonTransitionYear_Not12Months_FailsValidation()
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand(
                startDate: new DateOnly(2024, 4, 1),
                endDate:   new DateOnly(2025, 2, 28),
                isTransitionYear: false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("exactly 12 months"));
        }

        [Fact]
        public async Task Validate_TransitionYear_Bypasses12MonthRule()
        {
            var cmd = FinancialYearMasterBuilders.ValidCreateCommand(
                code: "2024-25",
                startDate: new DateOnly(2024, 4, 1),
                endDate:   new DateOnly(2025, 6, 30),    // 15 months
                isTransitionYear: true);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.Errors.Should().NotContain(e => e.ErrorMessage.Contains("exactly 12 months"));
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("2024-25", 1, It.IsAny<int?>())).ReturnsAsync(true);

            var validator = new CreateFinancialYearMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);

            var result = await validator.TestValidateAsync(FinancialYearMasterBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.FinancialYearCode);
        }

        [Fact]
        public async Task Validate_OverlappingRange_FailsValidation()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.OverlapsExistingRangeAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), 1, It.IsAny<int?>()))
                .ReturnsAsync(true);

            var validator = new CreateFinancialYearMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);

            var result = await validator.TestValidateAsync(FinancialYearMasterBuilders.ValidCreateCommand());
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("overlaps"));
        }
    }
}
