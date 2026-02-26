using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMapping
{
    public class UpdateDispatchAddressMappingCommandValidator : AbstractValidator<UpdateDispatchAddressMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMappingQueryRepository _queryRepo;

        public UpdateDispatchAddressMappingCommandValidator(IDispatchAddressMappingQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Dispatch Address Mapping {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateDispatchAddressMappingCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Business rule: Only one default mapping per (PartyId, UsageTypeId)
            // Fetch existing to get the immutable keys for the check
            RuleFor(x => x.IsDefault)
                .MustAsync(async (command, isDefault, ct) =>
                {
                    if (!isDefault) return true;
                    var existing = await _queryRepo.GetByIdAsync(command.Id);
                    if (existing == null) return true; // NotFound rule handles missing entity
                    return !await _queryRepo.DefaultAlreadyExistsAsync(existing.PartyId, existing.UsageTypeId, command.Id);
                })
                .WithMessage("A default Dispatch Address Mapping already exists for this Party and Usage Type.")
                .When(x => x.IsDefault && x.Id > 0);
        }
    }
}
