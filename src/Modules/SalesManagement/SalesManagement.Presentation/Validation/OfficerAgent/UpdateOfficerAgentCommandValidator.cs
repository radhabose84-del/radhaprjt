using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.OfficerAgent
{
    public class UpdateOfficerAgentCommandValidator : AbstractValidator<UpdateOfficerAgentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IOfficerAgentQueryRepository _queryRepository;

        public UpdateOfficerAgentCommandValidator(
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
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.AgentId)} {rule.Error}");

                        RuleFor(x => x.MarketingOfficerId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}");

                        RuleFor(x => x.ValidityFrom)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.ValidityFrom)} {rule.Error}");

                        RuleFor(x => x.ValidityTo)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.ValidityTo)} {rule.Error}")
                            .GreaterThanOrEqualTo(x => x.ValidityFrom)
                            .WithMessage("ValidityTo must be on or after ValidityFrom.")
                            .When(x => x.ValidityTo != default);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"OfficerAgent {rule.Error}");

                        // Expired assignment cannot be edited
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsExpiredAsync(id))
                            .WithMessage("Expired assignments cannot be edited.")
                            .When(x => x.Id > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) => await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateOfficerAgentCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
