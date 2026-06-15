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
            IArrivalLookup arrivalLookup,
            IItemLookup itemLookup)
        {
            // Resolves the source line's ItemId based on the source type (GRN or Arrival).
            async Task<int?> SourceItemIdAsync(CreateQcInspectionCommand cmd, CancellationToken ct)
            {
                var arrivalTypeId = await queryRepo.GetSourceTypeIdByCodeAsync("ARRIVAL");
                if (arrivalTypeId.HasValue && cmd.SourceTypeId == arrivalTypeId.Value)
                {
                    var a = await arrivalLookup.GetByArrivalDetailIdAsync(cmd.SourceDetailId, ct);
                    return a?.ItemId;
                }
                var g = await grnLookup.GetByGrnDetailIdAsync(cmd.SourceDetailId, ct);
                return g?.ItemId;
            }

            RuleFor(x => x.SourceTypeId)
                .GreaterThan(0).WithMessage("Source Type is required.");

            RuleFor(x => x.SourceDetailId)
                .GreaterThan(0).WithMessage("Source Detail ID is required.");

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) => await SourceItemIdAsync(cmd, ct) != null)
                .WithMessage("Source line item not found.")
                .When(x => x.SourceTypeId > 0 && x.SourceDetailId > 0);

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await queryRepo.InspectionExistsForSourceAsync(cmd.SourceTypeId, cmd.SourceDetailId))
                .WithMessage("Inspection already exists for this source line.")
                .When(x => x.SourceTypeId > 0 && x.SourceDetailId > 0);

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var itemId = await SourceItemIdAsync(cmd, ct);
                    if (itemId == null) return false;
                    var items = await itemLookup.GetByIdsAsync(new[] { itemId.Value }, ct);
                    var item = items.Count > 0 ? items[0] : null;
                    return item != null && item.InspectionRequired;
                })
                .WithMessage("QC is not required for this item.")
                .When(x => x.SourceTypeId > 0 && x.SourceDetailId > 0);
        }
    }
}
