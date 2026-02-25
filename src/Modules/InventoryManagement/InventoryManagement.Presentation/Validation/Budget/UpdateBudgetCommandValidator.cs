using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using FluentValidation;
using System.Text.RegularExpressions;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Budget
{
    public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
    {
        public UpdateBudgetCommandValidator()
        {
            var validationRules = ValidationRuleLoader.LoadValidationRules();

            foreach (var rule in validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.BudgetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateBudgetCommand.BudgetId)} {rule.Error}");
                        break;

                    case "NumericWithDecimal":
                        RuleForEach(x => x.BudgetDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.NewAmount.ToString())
                                    .Matches(new Regex(rule.Pattern))
                                    .WithMessage($"{nameof(UpdateBudgetDetailDto.NewAmount)} {rule.Error}");
                            });
                        break;

                    case "GreaterThanZero":
                        RuleForEach(x => x.BudgetDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.NewAmount)
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(UpdateBudgetDetailDto.NewAmount)} must be greater than zero.");
                            });
                        break;
                }
            }
        }
    }
}
