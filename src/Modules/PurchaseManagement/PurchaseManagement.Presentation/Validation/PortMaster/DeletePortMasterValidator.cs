
using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation;
public sealed class DeletePortMasterValidator : AbstractValidator<DeletePortMasterCommand>
{
    public DeletePortMasterValidator() => RuleFor(x => x.Id).GreaterThan(0);
}