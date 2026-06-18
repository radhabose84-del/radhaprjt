using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Presentation.Validation.ScheduleIII;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class CreateSubTotalCommandValidatorTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateSubTotalCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private static CreateSubTotalCommand ValidCommand() =>
            new() { FormulaName = "Gross Profit", IncludeOtherIncome = false, DisplayOrder = 1 };

        [Fact]
        public async Task Validate_Valid_Passes()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNameExistsAsync("Gross Profit", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SubTotalDisplayOrderExistsAsync(1, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyFormulaName_Fails()
        {
            var command = ValidCommand();
            command.FormulaName = "";

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FormulaName);
        }

        [Fact]
        public async Task Validate_DuplicateFormulaName_Fails()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNameExistsAsync("Gross Profit", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.FormulaName);
        }

        [Fact]
        public async Task Validate_DuplicateDisplayOrder_Fails()
        {
            _mockQueryRepo.Setup(r => r.SubTotalDisplayOrderExistsAsync(1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
        }
    }
}
