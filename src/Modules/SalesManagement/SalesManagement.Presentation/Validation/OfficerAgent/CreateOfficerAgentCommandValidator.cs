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
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.MarketingOfficerId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");

                        RuleFor(x => x.Agents)
                            .NotEmpty()
                            .WithMessage($"Agents {rule.Error}");

                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.AgentId)
                                .GreaterThan(0)
                                .WithMessage($"AgentId {rule.Error}");

                            agent.RuleFor(a => a.ValidityFrom)
                                .NotEqual(default(DateOnly))
                                .WithMessage($"ValidityFrom {rule.Error}");

                            // BUG-02 fix: separate rules so .When() doesn't skip NotEqual
                            agent.RuleFor(a => a.ValidityTo)
                                .NotEqual(default(DateOnly))
                                .WithMessage($"ValidityTo {rule.Error}");

                            agent.RuleFor(a => a.ValidityTo)
                                .GreaterThanOrEqualTo(a => a.ValidityFrom)
                                .WithMessage("ValidityTo must be on or after ValidityFrom.")
                                .When(a => a.ValidityTo != default);

                            // BUG-03 fix: reject if ValidityTo is already in the past
                            agent.RuleFor(a => a.ValidityTo)
                                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                                .WithMessage("ValidityTo must not be a past date.")
                                .When(a => a.ValidityTo != default);
                        });
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);

                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.AgentId)
                                .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                                .WithMessage($"AgentId {rule.Error}")
                                .When(a => a.AgentId > 0);
                        });
                        break;

                    case "ByteValue":
                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.IsActive)
                                .InclusiveBetween(0, 1)
                                .WithMessage($"{nameof(OfficerAgentBatchItem.IsActive)} {rule.Error}");
                        });
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
