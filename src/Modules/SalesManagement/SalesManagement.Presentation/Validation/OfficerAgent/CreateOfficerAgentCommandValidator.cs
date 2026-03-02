using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.OfficerAgent
{
    public class CreateOfficerAgentCommandValidator : AbstractValidator<CreateOfficerAgentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IOfficerAgentQueryRepository _queryRepository;

        public CreateOfficerAgentCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IOfficerAgentQueryRepository queryRepository)
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
                        RuleFor(x => x.AgentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.AgentId)} {rule.Error}");

                        RuleFor(x => x.MarketingOfficerId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");

                        RuleFor(x => x.ValidityFrom)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.ValidityFrom)} {rule.Error}");

                        RuleFor(x => x.ValidityTo)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.ValidityTo)} {rule.Error}")
                            .GreaterThanOrEqualTo(x => x.ValidityFrom)
                            .WithMessage("ValidityTo must be on or after ValidityFrom.")
                            .When(x => x.ValidityTo != default);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
