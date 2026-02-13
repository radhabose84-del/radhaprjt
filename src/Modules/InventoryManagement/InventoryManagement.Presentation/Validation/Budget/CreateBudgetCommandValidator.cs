using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Common.Interfaces.Budget;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using System.Text.RegularExpressions;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Budget
{
    public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
    {
        private readonly IBudgetCommandRepository _budgetRepository;

        public CreateBudgetCommandValidator(IBudgetCommandRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;

            var validationRules = ValidationRuleLoader.LoadValidationRules();

            foreach (var rule in validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.BudgetGroupId)
                            .NotEmpty()
                            .WithMessage(nameof(CreateBudgetCommand.BudgetGroupId) + rule.Error);

                        RuleFor(x => x.FiscalYear)
                            .NotEmpty()
                            .WithMessage(nameof(CreateBudgetCommand.FiscalYear) + rule.Error);

                        RuleFor(x => x.YearBudgetAmount)
                            .NotEmpty()
                            .WithMessage(nameof(CreateBudgetCommand.YearBudgetAmount) + rule.Error);
                        break;

                    case "NonNegativeInteger":
                        RuleFor(x => x.FiscalYear.ToString())
                            .Matches(new Regex(rule.Pattern))
                            .WithMessage(nameof(CreateBudgetCommand.FiscalYear) + " " + rule.Error);
                        break;
/* 
                    case "BetweenYear":
                        RuleFor(x => x.FiscalYear)
                            .InclusiveBetween(2000, 2100)
                            .WithMessage($"{nameof(CreateBudgetCommand.FiscalYear)} {rule.Error} 2000 and 2100.");
                        break; */

                    case "NumericWithDecimal":
                        RuleFor(x => x.YearBudgetAmount.ToString())
                            .Matches(new Regex(rule.Pattern))
                            .WithMessage(nameof(CreateBudgetCommand.YearBudgetAmount) + " " + rule.Error);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, cancellation) =>
                                !await _budgetRepository.ExistsAsync(cmd.BudgetGroupId, cmd.FiscalYear))
                            .WithMessage("Budget already exists for this Unit, Group, and Fiscal Year.");
                        break;
                }
            }
        }
    }
}
