using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
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
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CreateOfficerAgentCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IOfficerAgentQueryRepository queryRepository,
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

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => !await _accessFilter.ShouldApplyFilterAsync(ct)
                                        || id == _accessFilter.GetCurrentMarketingOfficerId())
                            .WithMessage($"{nameof(CreateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        // In-request duplicate — same AgentId listed twice in the same payload
                        RuleFor(x => x.Agents)
                            .Must(agents => agents.Select(a => a.AgentId).Distinct().Count() == agents.Count)
                            .WithMessage("Agent appears more than once in this request.")
                            .When(x => x.Agents != null
                                    && x.Agents.Count > 0
                                    && x.Agents.All(a => a.AgentId > 0));

                        // Cross-request duplicate — active assignment already exists in DB.
                        // Per-element guard: only run when both ids are valid (other rules cover the rest).
                        RuleForEach(x => x.Agents)
                            .MustAsync(async (cmd, agent, ct) =>
                                cmd.MarketingOfficerId <= 0
                                    || agent.AgentId <= 0
                                    || !await _queryRepository.AlreadyAssignedAsync(cmd.MarketingOfficerId, agent.AgentId))
                            .WithMessage("This agent is already assigned to this Marketing Officer.")
                            .When(x => x.Agents != null && x.Agents.Count > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
