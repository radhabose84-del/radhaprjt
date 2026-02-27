using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMapping
{
    public class DeleteDispatchAddressMappingCommandValidator : AbstractValidator<DeleteDispatchAddressMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;

        public DeleteDispatchAddressMappingCommandValidator(IDispatchAddressMappingQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteDispatchAddressMappingCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Dispatch Address Mapping {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
