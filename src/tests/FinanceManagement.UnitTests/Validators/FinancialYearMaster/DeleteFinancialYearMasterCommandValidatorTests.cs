using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster;
using FinanceManagement.Presentation.Validation.FinancialYearMaster;

namespace FinanceManagement.UnitTests.Validators.FinancialYearMaster
{
    public sealed class DeleteFinancialYearMasterCommandValidatorTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteFinancialYearMasterCommandValidator CreateValidator()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);
            return new DeleteFinancialYearMasterCommandValidator(_mockQueryRepo.Object);
        }

        [Fact]
        public async Task Validate_ValidId_Passes()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteFinancialYearMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteFinancialYearMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var validator = new DeleteFinancialYearMasterCommandValidator(_mockQueryRepo.Object);

            var result = await validator.TestValidateAsync(new DeleteFinancialYearMasterCommand(99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_SoftDeleteBlocksDueToLinks_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);
            var validator = new DeleteFinancialYearMasterCommandValidator(_mockQueryRepo.Object);

            var result = await validator.TestValidateAsync(new DeleteFinancialYearMasterCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
