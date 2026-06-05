using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO;

namespace PurchaseManagement.Presentation.Validation.RawMaterialPO
{
    public class DeleteRawMaterialPOCommandValidator : AbstractValidator<DeleteRawMaterialPOCommand>
    {
        public DeleteRawMaterialPOCommandValidator(IRawMaterialPOQueryRepository queryRepo)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.")
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("Raw Material PO not found.")
                .When(x => x.Id > 0);
        }
    }
}
