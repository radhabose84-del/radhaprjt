using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Presentation.Validation.ScheduleIII;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class UpdateSubTotalCommandValidatorTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateSubTotalCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupCommon(int id = 2)
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidFormula_Passes()
        {
            SetupCommon();
            var command = new UpdateSubTotalCommand
            {
                Id = 2,
                FormulaName = "EBITDA",
                IncludeOtherIncome = true,
                DisplayOrder = 3,
                IsActive = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyFormulaName_Fails()
        {
            SetupCommon();
            var command = new UpdateSubTotalCommand { Id = 2, FormulaName = "" };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FormulaName);
        }

        [Fact]
        public async Task Validate_DuplicateFormulaName_Fails()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.SubTotalNameExistsAsync("EBITDA", 2)).ReturnsAsync(true);
            var command = new UpdateSubTotalCommand { Id = 2, FormulaName = "EBITDA", DisplayOrder = 3, IsActive = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FormulaName);
        }

        [Fact]
        public async Task Validate_DuplicateDisplayOrder_Fails()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.SubTotalDisplayOrderExistsAsync(3, 2)).ReturnsAsync(true);
            var command = new UpdateSubTotalCommand { Id = 2, FormulaName = "EBITDA", DisplayOrder = 3, IsActive = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
        }
    }
}
