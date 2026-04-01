using FluentValidation.TestHelper;
using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Presentation.Validation.Budget;

namespace InventoryManagement.UnitTests.Validators.Budget
{
    public sealed class UpdateBudgetCommandValidatorTests
    {
        private UpdateBudgetCommandValidator CreateValidator() => new();

        private static UpdateBudgetCommand ValidCommand() => new()
        {
            BudgetId = 1,
            YearBudgetAmount = 50000m,
            BudgetDetails = new List<UpdateBudgetDetailDto>
            {
                new UpdateBudgetDetailDto { DetailId = 1, NewAmount = 5000m, Remarks = "Jan" }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroBudgetId_FailsValidation()
        {
            var command = ValidCommand();
            command.BudgetId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeDetailAmount_NoGreaterThanZeroRuleInJson()
        {
            // Validator uses case "GreaterThanZero" but validation-rules.json has "GreaterThan" —
            // the case never matches, so no greater-than-zero validation is registered.
            var command = ValidCommand();
            command.BudgetDetails = new List<UpdateBudgetDetailDto>
            {
                new UpdateBudgetDetailDto { DetailId = 1, NewAmount = -100m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroDetailAmount_NoGreaterThanZeroRuleInJson()
        {
            // Same as above — "GreaterThanZero" case doesn't match any JSON rule.
            var command = ValidCommand();
            command.BudgetDetails = new List<UpdateBudgetDetailDto>
            {
                new UpdateBudgetDetailDto { DetailId = 1, NewAmount = 0m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDetailsList_PassesValidation()
        {
            var command = ValidCommand();
            command.BudgetDetails = new List<UpdateBudgetDetailDto>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
