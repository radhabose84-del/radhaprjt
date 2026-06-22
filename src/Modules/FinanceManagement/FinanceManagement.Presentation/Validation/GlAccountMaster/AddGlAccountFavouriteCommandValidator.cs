using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite;

namespace FinanceManagement.Presentation.Validation.GlAccountMaster
{
    public class AddGlAccountFavouriteCommandValidator : AbstractValidator<AddGlAccountFavouriteCommand>
    {
        public AddGlAccountFavouriteCommandValidator(IGlAccountMasterQueryRepository queryRepository)
        {
            RuleFor(x => x.GlAccountMasterId)
                .GreaterThan(0).WithMessage("Valid GL account is required.")
                .MustAsync(async (id, ct) => !await queryRepository.NotFoundAsync(id))
                .WithMessage("GL Account not found.")
                .When(x => x.GlAccountMasterId > 0);
        }
    }
}
