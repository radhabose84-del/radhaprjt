using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation;

public sealed class CreatePortMasterValidator : AbstractValidator<CreatePortMasterCommand>
{
    public CreatePortMasterValidator(IPortMasterQueryRepository queryRepository)
    {
        RuleFor(x => x.PortCode).NotEmpty().MaximumLength(20).Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.PortName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.PortTypeId).GreaterThan(0);

        // Enforce PortCode uniqueness — previously missing, so duplicate codes were silently created (201).
        RuleFor(x => x.PortCode)
            .MustAsync(async (code, ct) => !await queryRepository.AlreadyExistsAsync(code, ct))
            .WithMessage("PortCode already exists.")
            .When(x => !string.IsNullOrWhiteSpace(x.PortCode));
    }
}

