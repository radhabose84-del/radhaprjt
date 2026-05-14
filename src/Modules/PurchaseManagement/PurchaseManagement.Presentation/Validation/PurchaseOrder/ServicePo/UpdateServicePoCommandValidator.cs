using FluentValidation;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    /// <summary>
    /// Wrapper so the same DTO-level rules (including the 3 ESR linkage rules)
    /// run on the Update path. Without this, the Update command pipeline skips validation
    /// entirely because ValidationBehavior resolves IValidator&lt;TCommand&gt;.
    /// </summary>
    public sealed class UpdateServicePoCommandValidator : AbstractValidator<UpdateServicePoCommand>
    {
        public UpdateServicePoCommandValidator(IValidator<CreateServicePurchaseOrderDto> dtoValidator)
        {
            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Request body is required.");

            RuleFor(x => x.Data!.Id)
                .GreaterThan(0)
                .WithMessage("Valid Service PO Id is required for update.")
                .When(x => x.Data != null);

            RuleFor(x => x.Data)
                .SetValidator(dtoValidator!)
                .When(x => x.Data != null);
        }
    }
}
