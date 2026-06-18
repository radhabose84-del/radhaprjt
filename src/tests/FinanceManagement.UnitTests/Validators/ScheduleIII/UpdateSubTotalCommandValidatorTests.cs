using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Presentation.Validation.ScheduleIII;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class UpdateSubTotalCommandValidatorTests
    {
        private const int SubTotalOperandTypeId = 141;
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateSubTotalCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupCommon(int id = 2)
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetSubTotalOperandTypeIdAsync()).ReturnsAsync(SubTotalOperandTypeId);
            _mockQueryRepo.Setup(r => r.SubTotalTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidFormula_Passes()
        {
            SetupCommon();
            var command = new UpdateSubTotalCommand
            {
                Id = 2,
                SubTotalTypeId = 29,
                IncludeOtherIncome = true,
                Formulas = new List<SubTotalFormulaInput>
                {
                    new() { OperandTypeId = 140, OperandRefId = 22, OperatorId = 131, DisplayOrder = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_SelfReference_Fails()
        {
            SetupCommon();
            var command = new UpdateSubTotalCommand
            {
                Id = 2,
                SubTotalTypeId = 29,
                Formulas = new List<SubTotalFormulaInput>
                {
                    // operand is a SUBTOTAL pointing at the same sub-total Id (2) -> self reference
                    new() { OperandTypeId = SubTotalOperandTypeId, OperandRefId = 2, OperatorId = 130, DisplayOrder = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("A sub-total cannot reference itself in its formula.");
        }

        [Fact]
        public async Task Validate_EmptyType_Fails()
        {
            SetupCommon();
            var command = new UpdateSubTotalCommand { Id = 2, SubTotalTypeId = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SubTotalTypeId);
        }
    }
}
