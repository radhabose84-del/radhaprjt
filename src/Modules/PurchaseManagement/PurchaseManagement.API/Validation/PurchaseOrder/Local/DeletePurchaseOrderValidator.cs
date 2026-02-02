using FluentValidation;

namespace PurchaseManagement.API.Validation.PurchaseOrder.Local;
public class DeletePurchaseOrderValidator : AbstractValidator<int>
{
    public DeletePurchaseOrderValidator()
    {
        RuleFor(x => x).GreaterThan(0);
    }
}