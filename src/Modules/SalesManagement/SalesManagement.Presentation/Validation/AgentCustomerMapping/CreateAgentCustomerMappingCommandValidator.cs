using FluentValidation;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCustomerMapping
{
    public class CreateAgentCustomerMappingCommandValidator
        : AbstractValidator<CreateAgentCustomerMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;

        public CreateAgentCustomerMappingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAgentCustomerMappingQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.CustomerId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.CustomerId)} {rule.Error}");

                        RuleFor(x => x.AgentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.AgentId)} {rule.Error}");

                        RuleFor(x => x.SalesSegmentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SalesSegmentId)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEqual(default(DateTime))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.EffectiveFrom)} {rule.Error}")
                            .LessThanOrEqualTo(_ => DateTime.Today)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.EffectiveFrom)} cannot be a future date.")
                            .When(x => x.EffectiveFrom != default);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) => await _queryRepository.CustomerExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.CustomerId)} {rule.Error}")
                            .When(x => x.CustomerId > 0);

                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        RuleFor(x => x.SubAgentId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.SubAgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SubAgentId)} {rule.Error}")
                            .When(x => x.SubAgentId.HasValue && x.SubAgentId.Value > 0);

                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SalesSegmentId)} {rule.Error}")
                            .When(x => x.SalesSegmentId > 0);
                        break;

                    case "AlreadyExists":
                        // BR-2: SubAgentId cannot equal AgentId
                        RuleFor(x => x.SubAgentId)
                            .Must((cmd, subAgentId) => subAgentId != cmd.AgentId)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SubAgentId)} cannot be the same as AgentId.")
                            .When(x => x.SubAgentId.HasValue);
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThan(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.EffectiveTo)} must be greater than EffectiveFrom.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
