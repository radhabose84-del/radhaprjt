using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.API.Validation;

public sealed class CreatePortMasterValidator : AbstractValidator<CreatePortMasterCommand>
{
    public CreatePortMasterValidator()
    {
        RuleFor(x => x.PortCode).NotEmpty().MaximumLength(20).Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.PortName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.PortTypeId).GreaterThan(0);        
    }
}

