using FluentValidation;
using SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCustomerMapping
{
    public class UpdateAgentCustomerMappingCommandValidator
        : AbstractValidator<UpdateAgentCustomerMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateAgentCustomerMappingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAgentCustomerMappingQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

            var maxLengthRemarks = maxLengthProvider
                .GetMaxLength<Domain.Entities.AgentCustomerMapping>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.AgentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.AgentId)} {rule.Error}");

                        RuleFor(x => x.SalesSegmentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.SalesSegmentId)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEqual(default(DateTime))
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.EffectiveFrom)} {rule.Error}")
                            .LessThanOrEqualTo(_ => DateTime.Today)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.EffectiveFrom)} cannot be a future date.")
                            .When(x => x.EffectiveFrom != default);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"AgentCustomerMapping {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        RuleFor(x => x.SubAgentId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.SubAgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.SubAgentId)} {rule.Error}")
                            .When(x => x.SubAgentId.HasValue && x.SubAgentId.Value > 0);

                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.SalesSegmentId)} {rule.Error}")
                            .When(x => x.SalesSegmentId > 0);
                        break;

                    case "AlreadyExists":
                        // BR-2: SubAgentId cannot equal AgentId
                        RuleFor(x => x.SubAgentId)
                            .Must((cmd, subAgentId) => subAgentId != cmd.AgentId)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.SubAgentId)} cannot be the same as AgentId.")
                            .When(x => x.SubAgentId.HasValue);
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThan(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.EffectiveTo)} must be greater than EffectiveFrom.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.IsActive)} {rule.Error}");
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessAgentAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCustomerMappingCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
