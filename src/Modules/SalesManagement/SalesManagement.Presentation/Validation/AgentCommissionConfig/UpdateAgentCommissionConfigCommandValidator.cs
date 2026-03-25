using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.AgentCommissionConfig
{
    public class UpdateAgentCommissionConfigCommandValidator : AbstractValidator<UpdateAgentCommissionConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;

        public UpdateAgentCommissionConfigCommandValidator(
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
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.AgentId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.SalesSegmentId)} {rule.Error}");

                        // CommissionTypeId required
                        RuleFor(x => x.CommissionTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}");

                        // CommissionPercentage range
                        RuleFor(x => x.CommissionPercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CommissionPercentage)} must be greater than or equal to 0.")
                            .LessThanOrEqualTo(100)
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CommissionPercentage)} must be less than or equal to 100.");

                        // ValidityFrom required
                        RuleFor(x => x.ValidityFrom)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.ValidityFrom)} {rule.Error}");

                        // ValidityTo required + must be >= ValidityFrom
                        RuleFor(x => x.ValidityTo)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.ValidityTo)} {rule.Error}")
                            .GreaterThanOrEqualTo(x => x.ValidityFrom)
                            .WithMessage("ValidityTo must be greater than or equal to ValidityFrom.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Agent Commission Configuration Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Agent Commission Configuration {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        // AgentId FK exists (cross-module)
                        RuleFor(x => x.AgentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AgentExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.AgentId)} {rule.Error}")
                            .When(x => x.AgentId > 0);

                        // SalesSegmentId FK exists (same-module)
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.SalesSegmentId)} {rule.Error}")
                            .When(x => x.SalesSegmentId > 0);

                        // CommissionTypeId FK exists (same-module MiscMaster)
                        RuleFor(x => x.CommissionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.CommissionTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CommissionTypeId)} {rule.Error}")
                            .When(x => x.CommissionTypeId > 0);

                        // CommissionBasisId FK exists (same-module MiscMaster, optional)
                        RuleFor(x => x.CommissionBasisId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.CommissionBasisExistsAsync(id))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CommissionBasisId)} {rule.Error}")
                            .When(x => x.CommissionBasisId.HasValue && x.CommissionBasisId.Value > 0);

                        // ApplicableLevelId FK exists (same-module MiscMaster, optional)
                        RuleFor(x => x.ApplicableLevelId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.ApplicableLevelExistsAsync(id))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.ApplicableLevelId)} {rule.Error}")
                            .When(x => x.ApplicableLevelId.HasValue && x.ApplicableLevelId.Value > 0);

                        // CurrencyId FK exists (cross-module, optional)
                        RuleFor(x => x.CurrencyId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);
                        break;

                    case "AlreadyExists":
                        // Overlap check — exclude self (current Id)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.AgentId,
                                    cmd.SalesSegmentId,
                                    cmd.ValidityFrom,
                                    cmd.ValidityTo,
                                    excludeId: cmd.Id))
                            .WithMessage("An active commission rule already exists for this Agent and Sales Segment within the specified validity period.")
                            .When(x => x.Id > 0 && x.AgentId > 0 && x.SalesSegmentId > 0
                                        && x.ValidityFrom != default && x.ValidityTo != default
                                        && x.ValidityTo >= x.ValidityFrom);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAgentCommissionConfigCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
