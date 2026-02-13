using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation;
public sealed class UpdatePortMasterValidator : AbstractValidator<UpdatePortMasterCommand>
{
    public UpdatePortMasterValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PortCode).NotEmpty().MaximumLength(20).Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.PortName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.PortTypeId).GreaterThan(0); 
    }
}