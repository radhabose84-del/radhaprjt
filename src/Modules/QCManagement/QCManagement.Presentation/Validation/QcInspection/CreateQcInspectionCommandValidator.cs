using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.CreateQcInspection;

namespace QCManagement.Presentation.Validation.QcInspection
{
    public class CreateQcInspectionCommandValidator : AbstractValidator<CreateQcInspectionCommand>
    {
        public CreateQcInspectionCommandValidator(
            IQcInspectionQueryRepository queryRepo,
            IGrnLookup grnLookup,
            IItemLookup itemLookup)
        {
            RuleFor(x => x.GrnDetailId)
                .GreaterThan(0).WithMessage("GRN Detail ID is required.");

            RuleFor(x => x.GrnDetailId)
                .MustAsync(async (id, ct) => await grnLookup.GetByGrnDetailIdAsync(id, ct) != null)
                .WithMessage("GRN line item not found.")
                .When(x => x.GrnDetailId > 0);

            RuleFor(x => x.GrnDetailId)
                .MustAsync(async (id, ct) => !await queryRepo.InspectionExistsForGrnDetailAsync(id))
                .WithMessage("Inspection already exists for this GRN line.")
                .When(x => x.GrnDetailId > 0);

            RuleFor(x => x.GrnDetailId)
                .MustAsync(async (id, ct) =>
                {
                    var grn = await grnLookup.GetByGrnDetailIdAsync(id, ct);
                    if (grn == null) return false;
                    var item = (await itemLookup.GetByIdsAsync(new[] { grn.ItemId }, ct)).FirstOrDefault();
                    return item != null && item.InspectionRequired;
                })
                .WithMessage("QC is not required for this item.")
                .When(x => x.GrnDetailId > 0);
        }
    }
}
