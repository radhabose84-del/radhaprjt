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
                        RuleFor(x => x.AgentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.AgentId)} {rule.Error}");

                        RuleFor(x => x.CommissionTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}");

                        RuleFor(x => x.CommissionBasisId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionBasisId)} {rule.Error}");

                        RuleFor(x => x.ApplicableLevelId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ApplicableLevelId)} {rule.Error}");

                        RuleFor(x => x.TriggerEventId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.TriggerEventId)} {rule.Error}");

                        RuleFor(x => x.CommissionSplitId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionSplitId)} {rule.Error}");

                        RuleFor(x => x.ValidityFrom)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ValidityFrom)} {rule.Error}");

                        RuleFor(x => x.Slabs)
                            .NotNull()
                            .WithMessage("At least one slab is required.")
                            .Must(s => s != null && s.Any())
                            .WithMessage("At least one slab is required.");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.CommissionPercentage)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionPercentage)} must be greater than zero.");

                        // Slab-level validations
                        RuleForEach(x => x.Slabs).ChildRules(slab =>
                        {
                            slab.RuleFor(s => s.CommissionTypeId)
                                .GreaterThan(0)
                                .WithMessage("Slab CommissionTypeId must be greater than zero.");

                            slab.RuleFor(s => s.CommissionBasisId)
                                .GreaterThan(0)
                                .WithMessage("Slab CommissionBasisId must be greater than zero.");

                            slab.RuleFor(s => s.CommissionValue)
                                .GreaterThan(0)
                                .WithMessage("Slab CommissionValue must be greater than zero.");
                        });
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.Slabs).ChildRules(slab =>
                        {
                            slab.RuleFor(s => s.FromDelay)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage("Slab FromDelay must be zero or positive.");
                        });
                        break;

                    case "DateCompare":
                        RuleFor(x => x.ValidityTo)
                            .GreaterThanOrEqualTo(x => x.ValidityFrom)
                            .WithMessage("ValidityTo must be greater than or equal to ValidityFrom.")
                            .When(x => x.ValidityTo.HasValue);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        RuleFor(x => x.CommissionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}")
                            .When(x => x.CommissionTypeId > 0);

                        RuleFor(x => x.CommissionBasisId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionBasisId)} {rule.Error}")
                            .When(x => x.CommissionBasisId > 0);

                        RuleFor(x => x.ApplicableLevelId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.ApplicableLevelId)} {rule.Error}")
                            .When(x => x.ApplicableLevelId > 0);

                        RuleFor(x => x.TriggerEventId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.TriggerEventId)} {rule.Error}")
                            .When(x => x.TriggerEventId > 0);

                        RuleFor(x => x.SlabTypeId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.SlabTypeId)} {rule.Error}")
                            .When(x => x.SlabTypeId.HasValue && x.SlabTypeId.Value > 0);

                        RuleFor(x => x.CommissionSplitId)
                            .MustAsync(async (id, ct) => await _queryRepository.CommissionSplitExistsAsync(id))
                            .WithMessage($"{nameof(CreateAgentCommissionConfigCommand.CommissionSplitId)} {rule.Error}")
                            .When(x => x.CommissionSplitId > 0);

                        // SalesGroupIds — each must exist
                        RuleForEach(x => x.SalesGroupIds)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesGroupExistsAsync(id))
                            .WithMessage("SalesGroupId {PropertyValue} is inactive/deleted.")
                            .When(x => x.SalesGroupIds != null && x.SalesGroupIds.Any());

                        // PaymentTermIds — each must exist (cross-module)
                        RuleForEach(x => x.PaymentTermIds)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermExistsAsync(id, ct))
                            .WithMessage("PaymentTermId {PropertyValue} is inactive/deleted.")
                            .When(x => x.PaymentTermIds != null && x.PaymentTermIds.Any());

                        // Slab FK validation — CommissionTypeId and CommissionBasisId must exist in MiscMaster
                        RuleForEach(x => x.Slabs).ChildRules(slab =>
                        {
                            slab.RuleFor(s => s.CommissionTypeId)
                                .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                                .WithMessage("Slab CommissionTypeId is inactive/deleted.")
                                .When(s => s.CommissionTypeId > 0);

                            slab.RuleFor(s => s.CommissionBasisId)
                                .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                                .WithMessage("Slab CommissionBasisId is inactive/deleted.")
                                .When(s => s.CommissionBasisId > 0);
                        });
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.AgentId,
                                    cmd.CommissionSplitId,
                                    cmd.ValidityFrom,
                                    cmd.ValidityTo))
                            .WithMessage("An active commission rule already exists for this Agent and Commission Split within the specified validity period.")
                            .When(x => x.AgentId > 0 && x.CommissionSplitId > 0
                                        && x.ValidityFrom != default);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
