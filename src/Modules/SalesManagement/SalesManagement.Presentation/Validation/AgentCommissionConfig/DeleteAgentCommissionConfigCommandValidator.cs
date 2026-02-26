using FluentValidation;
using SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCommissionConfig
{
    public class DeleteAgentCommissionConfigCommandValidator : AbstractValidator<DeleteAgentCommissionConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;

        public DeleteAgentCommissionConfigCommandValidator(IAgentCommissionConfigQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteAgentCommissionConfigCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Agent Commission Configuration {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
