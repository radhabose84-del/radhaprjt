using FluentValidation;
using SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCustomerMapping
{
    public class DeleteAgentCustomerMappingCommandValidator
        : AbstractValidator<DeleteAgentCustomerMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;

        public DeleteAgentCustomerMappingCommandValidator(
            IAgentCustomerMappingQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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

                    default:
                        break;
                }
            }
        }
    }
}
