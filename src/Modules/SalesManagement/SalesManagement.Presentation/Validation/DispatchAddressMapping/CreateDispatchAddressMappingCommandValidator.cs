using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMapping
{
    public class CreateDispatchAddressMappingCommandValidator : AbstractValidator<CreateDispatchAddressMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMappingQueryRepository _queryRepo;
        private readonly IPartyLookup _partyLookup;

        public CreateDispatchAddressMappingCommandValidator(
            IDispatchAddressMappingQueryRepository queryRepo,
            IPartyLookup partyLookup)
        {
            _queryRepo = queryRepo;
            _partyLookup = partyLookup;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.PartyId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.PartyId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.DispatchAddressId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.DispatchAddressId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.DispatchAddressId)} {rule.Error}");

                        RuleFor(x => x.UsageTypeId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.UsageTypeId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMappingCommand.UsageTypeId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (partyId, ct) => await _partyLookup.GetByIdAsync(partyId, ct) != null)
                            .WithMessage($"{nameof(CreateDispatchAddressMappingCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId > 0);

                        RuleFor(x => x.DispatchAddressId)
                            .MustAsync(async (dispatchAddressId, ct) => await _queryRepo.DispatchAddressExistsAsync(dispatchAddressId))
                            .WithMessage($"{nameof(CreateDispatchAddressMappingCommand.DispatchAddressId)} {rule.Error}")
                            .When(x => x.DispatchAddressId > 0);

                        RuleFor(x => x.UsageTypeId)
                            .MustAsync(async (usageTypeId, ct) => await _queryRepo.MiscMasterExistsAsync(usageTypeId))
                            .WithMessage($"{nameof(CreateDispatchAddressMappingCommand.UsageTypeId)} {rule.Error}")
                            .When(x => x.UsageTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (command, partyId, ct) =>
                                !await _queryRepo.CompositeKeyExistsAsync(command.PartyId, command.DispatchAddressId, command.UsageTypeId))
                            .WithMessage($"This Dispatch Address Mapping combination {rule.Error}")
                            .When(x => x.PartyId > 0 && x.DispatchAddressId > 0 && x.UsageTypeId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Business rule: Only one default mapping per (PartyId, UsageTypeId)
            RuleFor(x => x.IsDefault)
                .MustAsync(async (command, isDefault, ct) =>
                    !isDefault || !await _queryRepo.DefaultAlreadyExistsAsync(command.PartyId, command.UsageTypeId))
                .WithMessage("A default Dispatch Address Mapping already exists for this Party and Usage Type.")
                .When(x => x.IsDefault && x.PartyId > 0 && x.UsageTypeId > 0);
        }
    }
}
