using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Presentation.Validation.ScheduleIII;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class SaveSubTotalFormulaCommandValidatorTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private SaveSubTotalFormulaCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private static SaveSubTotalFormulaCommand ValidCommand() =>
            new()
            {
                SubTotalId = 2,
                Formulas = new List<SubTotalFormulaInput>
                {
                    new() { OperandTypeId = 26, SectionItemId = 17, OperatorId = 24, DisplayOrder = 1 }
                }
            };

        [Fact]
        public async Task Validate_Valid_Passes()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(2)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_SubTotalNotFound_Fails()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(99)).ReturnsAsync(true);
            var command = ValidCommand();
            command.SubTotalId = 99;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SubTotalId);
        }

        [Fact]
        public async Task Validate_EmptyFormulas_Fails()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(2)).ReturnsAsync(false);
            var command = ValidCommand();
            command.Formulas = new List<SubTotalFormulaInput>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Formulas);
        }
    }
}
