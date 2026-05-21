using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.GateInward
{
    public class CreateGateInwardCommandValidator : AbstractValidator<CreateGateInwardCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGateInwardQueryRepository _queryRepository;
        private readonly IPartyLookup _partyLookup;

        public CreateGateInwardCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGateInwardQueryRepository queryRepository,
            IPartyLookup partyLookup)
        {
            _queryRepository = queryRepository;
            _partyLookup = partyLookup;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.GateInwardHdr>("Remarks") ?? 250;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.VehicleMovementRecordId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.VehicleMovementRecordId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.VehicleMovementRecordId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.PartyId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.UnitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.GateInwardDetails)
                            .NotNull().WithMessage($"GateInwardDetails {rule.Error}")
                            .NotEmpty().WithMessage($"GateInwardDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateGateInwardCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.VehicleMovementRecordId)
                            .MustAsync(async (id, ct) => await _queryRepository.VehicleMovementRecordExistsAsync(id))
                            .WithMessage($"{nameof(CreateGateInwardCommand.VehicleMovementRecordId)} {rule.Error}")
                            .When(x => x.VehicleMovementRecordId > 0);

                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) => await _partyLookup.GetByIdAsync(id!.Value, ct) != null)
                            .WithMessage($"{nameof(CreateGateInwardCommand.PartyId)} {rule.Error}")
                            .When(x => x.PartyId.HasValue && x.PartyId > 0);

                        RuleFor(x => x.UnitId)
                            .MustAsync(async (id, ct) => await _queryRepository.UnitExistsAsync(id))
                            .WithMessage($"{nameof(CreateGateInwardCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);

                        RuleFor(x => x.QAStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateGateInwardCommand.QAStatusId)} {rule.Error}")
                            .When(x => x.QAStatusId.HasValue && x.QAStatusId > 0);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.GrossWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateGateInwardCommand.GrossWeight)} {rule.Error}")
                            .When(x => x.GrossWeight.HasValue);

                        RuleFor(x => x.TareWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateGateInwardCommand.TareWeight)} {rule.Error}")
                            .When(x => x.TareWeight.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
