using FluentValidation;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    /// <summary>
    /// Thin wrapper so that MediatR's ValidationBehavior (which validates the TRequest type)
    /// runs the existing DTO-level validator against the command's Data payload.
    /// Without this, IValidator&lt;CreateServicePurchaseOrderDto&gt; would never be invoked
    /// for the CreateServicePoCommand pipeline.
    /// </summary>
    public sealed class CreateServicePoCommandValidator : AbstractValidator<CreateServicePoCommand>
    {
        public CreateServicePoCommandValidator(IValidator<CreateServicePurchaseOrderDto> dtoValidator)
        {
            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Request body is required.");

            RuleFor(x => x.Data)
                .SetValidator(dtoValidator!)
                .When(x => x.Data != null);
        }
    }
}
