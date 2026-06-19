using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries;
using FinanceManagement.Presentation.Validation.VoucherType;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.VoucherType
{
    public sealed class ResetVoucherTypeSeriesCommandValidatorTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);

        private ResetVoucherTypeSeriesCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockFyLookup.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockFyLookup.Setup(f => f.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
        }

        private static ResetVoucherTypeSeriesCommand ValidCommand() =>
            new() { VoucherTypeId = 1, FinancialYearId = 3 };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroVoucherTypeId_Fails()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new ResetVoucherTypeSeriesCommand { VoucherTypeId = 0, FinancialYearId = 3 });
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeId);
        }

        [Fact]
        public async Task Validate_NonExistentVoucherType_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new ResetVoucherTypeSeriesCommand { VoucherTypeId = 99, FinancialYearId = 3 });
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeId);
        }

        [Fact]
        public async Task Validate_InvalidFinancialYear_Fails()
        {
            SetupHappyPath();
            _mockFyLookup.Setup(f => f.GetByIdAsync(777, It.IsAny<CancellationToken>())).ReturnsAsync((FinancialYearLookupDto?)null);

            var result = await CreateValidator().TestValidateAsync(new ResetVoucherTypeSeriesCommand { VoucherTypeId = 1, FinancialYearId = 777 });
            result.ShouldHaveValidationErrorFor(x => x.FinancialYearId);
        }
    }
}
