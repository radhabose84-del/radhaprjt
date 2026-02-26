using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCommissionConfig
{
    public class CreateAgentCommissionConfigCommandValidator : AbstractValidator<CreateAgentCommissionConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;

        public CreateAgentCommissionConfigCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAgentCommissionConfigQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // AgentId required
                        RuleFor(x => x.AgentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.AgentId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.SalesSegmentId)} {rule.Error}");

                        // ItemId required
                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ItemId)} {rule.Error}");

                        // CommissionTypeId required
                        RuleFor(x => x.CommissionTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}");

                        // CommissionPercentage >= 0
                        RuleFor(x => x.CommissionPercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionPercentage)} must be greater than or equal to 0.")
                            .LessThanOrEqualTo(100)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionPercentage)} must be less than or equal to 100.");

                        // ValidityFrom required
                        RuleFor(x => x.ValidityFrom)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ValidityFrom)} {rule.Error}");

                        // ValidityTo required + must be >= ValidityFrom
                        RuleFor(x => x.ValidityTo)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ValidityTo)} {rule.Error}")
                            .GreaterThanOrEqualTo(x => x.ValidityFrom)
                            .WithMessage("ValidityTo must be greater than or equal to ValidityFrom.");

                        // SubAgentPercentage <= CommissionPercentage (when provided)
                        RuleFor(x => x.SubAgentPercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.SubAgentPercentage)} must be greater than or equal to 0.")
                            .When(x => x.SubAgentPercentage.HasValue);

                        RuleFor(x => x.SubAgentPercentage)
                            .LessThanOrEqualTo(x => x.CommissionPercentage)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.SubAgentPercentage)} must be less than or equal to CommissionPercentage.")
                            .When(x => x.SubAgentPercentage.HasValue);
                        break;

                    case "FKColumnDelete":
                        // AgentId FK exists (cross-module)
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        // SalesSegmentId FK exists (same-module)
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.SalesSegmentId)} {rule.Error}")
                            .When(x => x.SalesSegmentId > 0);

                        // ItemId FK exists (cross-module)
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        // CommissionTypeId FK exists (same-module MiscMaster)
                        RuleFor(x => x.CommissionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.CommissionTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}")
                            .When(x => x.CommissionTypeId > 0);

                        // UomId FK exists (cross-module, optional)
                        RuleFor(x => x.UomId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.UomExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.UomId)} {rule.Error}")
                            .When(x => x.UomId.HasValue && x.UomId.Value > 0);

                        // CurrencyId FK exists (cross-module, optional)
                        RuleFor(x => x.CurrencyId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);
                        break;

                    case "AlreadyExists":
                        // Overlap check (same Agent + SalesSegment + Item with overlapping dates)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.AgentId,
                                    cmd.SalesSegmentId,
                                    cmd.ItemId,
                                    cmd.ValidityFrom,
                                    cmd.ValidityTo))
                            .WithMessage("An active commission rule already exists for this Agent, Sales Segment, and Item within the specified validity period.")
                            .When(x => x.AgentId > 0 && x.SalesSegmentId > 0 && x.ItemId > 0
                                        && x.ValidityFrom != default && x.ValidityTo != default
                                        && x.ValidityTo >= x.ValidityFrom);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
