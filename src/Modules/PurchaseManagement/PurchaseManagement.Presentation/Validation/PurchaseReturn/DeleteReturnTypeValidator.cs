using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class DeleteReturnTypeValidator : AbstractValidator<DeleteReturnTypeCommand>
{
    public DeleteReturnTypeValidator(IReturnTypeQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                    .WithMessage("Return Type not found.");

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                    .WithMessage("This master is linked with other records. You cannot delete this record.");
            });
    }
}
