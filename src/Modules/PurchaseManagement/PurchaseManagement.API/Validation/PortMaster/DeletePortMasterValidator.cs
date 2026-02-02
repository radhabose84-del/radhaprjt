
using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.API.Validation;
public sealed class DeletePortMasterValidator : AbstractValidator<DeletePortMasterCommand>
{
    public DeletePortMasterValidator() => RuleFor(x => x.Id).GreaterThan(0);
}