using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.OfficerAgent
{
    public class UpdateOfficerAgentCommandValidator : AbstractValidator<UpdateOfficerAgentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateOfficerAgentCommandValidator(
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
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");

                        RuleFor(x => x.Agents)
                            .NotEmpty()
                            .WithMessage($"Agents list {rule.Error}");

                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.Id)
                                .GreaterThan(0)
                                .WithMessage($"Assignment Id {rule.Error}");

                            agent.RuleFor(a => a.AgentId)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(OfficerAgentUpdateItem.AgentId)} {rule.Error}");

                            agent.RuleFor(a => a.ValidityFrom)
                                .NotEqual(default(DateOnly))
                                .WithMessage($"{nameof(OfficerAgentUpdateItem.ValidityFrom)} {rule.Error}");

                            agent.RuleFor(a => a.ValidityTo)
                                .NotEqual(default(DateOnly))
                                .WithMessage($"{nameof(OfficerAgentUpdateItem.ValidityTo)} {rule.Error}")
                                .GreaterThanOrEqualTo(a => a.ValidityFrom)
                                .WithMessage("ValidityTo must be on or after ValidityFrom.")
                                .When(a => a.ValidityTo != default);
                        });
                        break;

                    case "NotFound":
                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.Id)
                                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                                .WithMessage($"OfficerAgent {rule.Error}")
                                .When(a => a.Id > 0);
                        });

                        // SCRUM-1559: only block expired rows when the user is actually changing them.
                        // A round-tripped, untouched expired row must pass so unrelated saves aren't blocked.
                        RuleForEach(x => x.Agents)
                            .MustAsync(async (item, ct) =>
                                item.Id <= 0
                                    || !await _queryRepository.IsExpiredAndModifiedAsync(
                                        item.Id, item.AgentId, item.ValidityFrom, item.ValidityTo, item.IsActive))
                            .WithMessage("Expired assignments cannot be edited.")
                            .When(x => x.Agents != null && x.Agents.Count > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);

                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.AgentId)
                                .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                                .WithMessage($"{nameof(OfficerAgentUpdateItem.AgentId)} {rule.Error}")
                                .When(a => a.AgentId > 0);
                        });
                        break;

                    case "ByteValue":
                        RuleForEach(x => x.Agents).ChildRules(agent =>
                        {
                            agent.RuleFor(a => a.IsActive)
                                .InclusiveBetween(0, 1)
                                .WithMessage($"{nameof(OfficerAgentUpdateItem.IsActive)} {rule.Error}");
                        });
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => !await _accessFilter.ShouldApplyFilterAsync(ct)
                                        || id == _accessFilter.GetCurrentMarketingOfficerId())
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        // In-request duplicate — same AgentId listed twice in the same payload
                        RuleFor(x => x.Agents)
                            .Must(agents => agents.Select(a => a.AgentId).Distinct().Count() == agents.Count)
                            .WithMessage("Agent appears more than once in this request.")
                            .When(x => x.Agents != null
                                    && x.Agents.Count > 0
                                    && x.Agents.All(a => a.AgentId > 0));

                        // Cross-request duplicate — active assignment exists in DB,
                        // excluding the row being edited (matched by Id).
                        // Per-element guard: only run when both ids are valid.
                        RuleForEach(x => x.Agents)
                            .MustAsync(async (cmd, agent, ct) =>
                                cmd.MarketingOfficerId <= 0
                                    || agent.AgentId <= 0
                                    || !await _queryRepository.AlreadyAssignedAsync(
                                        cmd.MarketingOfficerId, agent.AgentId, agent.Id))
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
