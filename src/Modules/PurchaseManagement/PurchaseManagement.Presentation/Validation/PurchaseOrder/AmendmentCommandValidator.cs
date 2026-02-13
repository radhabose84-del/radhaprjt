using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using FluentValidation;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Amend;

public sealed class AmendmentCommandValidator : AbstractValidator<POAmendmentCommand>
{
    private readonly IPurchaseOrderQueryRepository _q;

    public AmendmentCommandValidator(IPurchaseOrderQueryRepository q)
    {
        _q = q;

        RuleLevelCascadeMode = CascadeMode.Stop;   // stop at first failure for each rule

        // Basic shape
        RuleFor(c => c.Data.Id)
            .GreaterThan(0).WithMessage("Invalid Purchase Order Id.");

        RuleFor(c => c.Data.AmendmentReason)
            .NotEmpty().WithMessage("Amendment reason is required.");

        // Domain guards moved from handler to validator:
        RuleFor(c => c.Data.Id)
            .MustAsync(ExistsAsync)
                .WithMessage("Purchase Order not found.")
            .MustAsync(NoGrnAsync)
                .WithMessage("GRN exists for this PO. Amendment is not allowed.")
            .MustAsync(IsApprovedStatusAsync)
                .WithMessage("Only Approved POs can be amended. Pending POs should be edited directly.");

        // (Optional) validate minimal payload shape you expect
        RuleFor(c => c.Data)
            .NotNull().WithMessage("Amendment payload is required.");
    }

    private Task<bool> ExistsAsync(int poId, CancellationToken ct)
        => _q.ExistsAsync(poId, ct);

    private async Task<bool> NoGrnAsync(int poId, CancellationToken ct)
    {
        var hasGrn = await _q.HasAnyGrnAsync(poId, ct);
        return !hasGrn;
    }

    private async Task<bool> IsApprovedStatusAsync(int poId, CancellationToken ct)
    {
        var status = await _q.GetStatusCodeAsync(poId, ct);
        return string.Equals(status, "Approved", System.StringComparison.OrdinalIgnoreCase);
    }
}
