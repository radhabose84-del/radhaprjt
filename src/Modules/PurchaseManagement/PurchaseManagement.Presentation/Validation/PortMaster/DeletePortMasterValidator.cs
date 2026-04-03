using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation;
public sealed class DeletePortMasterValidator : AbstractValidator<DeletePortMasterCommand>
{
    public DeletePortMasterValidator(IPortMasterQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id, ct))
                    .WithMessage("Port Master not found.");

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                    .WithMessage("This master is linked with other records. You cannot delete this record.");
            });
    }
}
