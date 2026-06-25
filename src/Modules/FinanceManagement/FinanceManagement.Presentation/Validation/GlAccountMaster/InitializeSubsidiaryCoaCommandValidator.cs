using FinanceManagement.Application.GlAccountMaster.Commands.InitializeSubsidiaryCoa;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.GlAccountMaster
{
    // US-GL02-10 (AC1) — bespoke guard for the subsidiary COA-inherit action.
    public class InitializeSubsidiaryCoaCommandValidator : AbstractValidator<InitializeSubsidiaryCoaCommand>
    {
        public InitializeSubsidiaryCoaCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("A valid subsidiary company is required.");
        }
    }
}
