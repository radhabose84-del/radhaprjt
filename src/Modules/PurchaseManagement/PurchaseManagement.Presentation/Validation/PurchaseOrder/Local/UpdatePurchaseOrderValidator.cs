using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using FluentValidation;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;
public class UpdatePurchaseOrderValidator : AbstractValidator<PurchaseOrderUpdateDto>
{
    public UpdatePurchaseOrderValidator(IPurchaseOrderQueryRepository q)
    {
        RuleFor(x => x.Id).GreaterThan(0);
        Include(new CreatePurchaseOrderValidator());

          RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid PO id.")
            .MustAsync(async (id, ct) => await q.ExistsAsync(id, ct))
            .WithMessage("Purchase Order not found.");

        
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) => !await q.HasAnyGrnAsync(id, ct))
            .WithMessage("GRN exists for this PO. Editing is not allowed.");

        // ✅ Only Pending POs can be edited. Approved must go through Amendment instead.
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
            {
                var code = await q.GetStatusCodeAsync(id, ct);
                return string.Equals(code, "Pending", StringComparison.OrdinalIgnoreCase);
            })
            .WithMessage("Only 'Pending' POs can be edited. Use Amendment for 'Approved' POs.");

    }
}
