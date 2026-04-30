using FluentValidation;
using SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCustomerMapping
{
    public class DeleteAgentCustomerMappingCommandValidator
        : AbstractValidator<DeleteAgentCustomerMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public DeleteAgentCustomerMappingCommandValidator(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteAgentCustomerMappingCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"AgentCustomerMapping {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id, ct))
                            .WithMessage("Cannot delete: Agent Customer Mapping is referenced in active transactions.");
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) =>
                            {
                                if (!await _accessFilter.ShouldApplyFilterAsync(ct))
                                    return true;
                                var record = await _queryRepository.GetByIdAsync(id);
                                return record != null;
                            })
                            .WithMessage("You are not authorized to delete this record.")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
