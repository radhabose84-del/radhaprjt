using FluentValidation;
using FinanceManagement.Application.GlAccountMaster.Commands.RemoveGlAccountFavourite;

namespace FinanceManagement.Presentation.Validation.GlAccountMaster
{
    public class RemoveGlAccountFavouriteCommandValidator : AbstractValidator<RemoveGlAccountFavouriteCommand>
    {
        public RemoveGlAccountFavouriteCommandValidator()
        {
            // Un-starring a non-existent favourite is a harmless no-op, so only the id shape is checked.
            RuleFor(x => x.GlAccountMasterId)
                .GreaterThan(0).WithMessage("Valid GL account is required.");
        }
    }
}
