using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces;
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
        private readonly IMarketingOfficerAccessFilter _accessFilter;
        private readonly ICustomerLookup _customerLookup;
        private readonly IAgentLookup _agentLookup;

        public CreateAgentCustomerMappingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAgentCustomerMappingQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter,
            ICustomerLookup customerLookup,
            IAgentLookup agentLookup)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;
            _customerLookup = customerLookup;
            _agentLookup = agentLookup;

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

                        RuleFor(x => x.SalesGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SalesGroupId)} {rule.Error}");

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

                        RuleFor(x => x.SalesGroupId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesGroupExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SalesGroupId)} {rule.Error}")
                            .When(x => x.SalesGroupId > 0);
                        break;

                    case "AlreadyExists":
                        // BR-2: SubAgentId cannot equal AgentId
                        RuleFor(x => x.SubAgentId)
                            .Must((cmd, subAgentId) => subAgentId != cmd.AgentId)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.SubAgentId)} cannot be the same as AgentId.")
                            .When(x => x.SubAgentId.HasValue);

                        // BR-3: Duplicate Customer + Agent combination not allowed
                        RuleFor(x => x)
                            .CustomAsync(async (cmd, context, ct) =>
                            {
                                if (cmd.CustomerId <= 0 || cmd.AgentId <= 0) return;

                                var exists = await _queryRepository.MappingAlreadyExistsAsync(cmd.CustomerId, cmd.AgentId, ct);
                                if (!exists) return;

                                var customers = await _customerLookup.GetAllCustomerAsync();
                                var customer = customers.FirstOrDefault(x => x.Id == cmd.CustomerId);

                                var agents = await _agentLookup.GetAllAgentAsync();
                                var agent = agents.FirstOrDefault(x => x.Id == cmd.AgentId);

                                var customerName = customer?.CustomerName ?? cmd.CustomerId.ToString();
                                var agentName = agent?.AgentName ?? cmd.AgentId.ToString();

                                context.AddFailure(
                                    $"Customer '{customerName}' is already mapped to Agent '{agentName}'. Duplicate mapping is not allowed.");
                            });
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThan(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.EffectiveTo)} must be greater than EffectiveFrom.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessAgentAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCustomerMappingCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
