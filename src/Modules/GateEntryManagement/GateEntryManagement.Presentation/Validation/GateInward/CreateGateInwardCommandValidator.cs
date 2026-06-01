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
            var maxLengthCourierNumber = maxLengthProvider.GetMaxLength<Domain.Entities.GateInwardHdr>("CourierNumber") ?? 50;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // PartyId is now OPTIONAL — frontend may send null (future-use field).
                        RuleFor(x => x.VehicleMovementRecordId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.VehicleMovementRecordId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.VehicleMovementRecordId)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.UnitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.ReceivingWarehouseId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.ReceivingWarehouseId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.ReceivingWarehouseId)} {rule.Error}");

                        RuleFor(x => x.ReceivingTypeId)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.ReceivingTypeId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.ReceivingTypeId)} {rule.Error}");

                        // CourierNumber is required only when ReceivingType resolves to 'Courier'
                        RuleFor(x => x.CourierNumber)
                            .NotNull().WithMessage($"{nameof(CreateGateInwardCommand.CourierNumber)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGateInwardCommand.CourierNumber)} {rule.Error}")
                            .WhenAsync(async (cmd, ct) =>
                                cmd.ReceivingTypeId.HasValue
                                && await _queryRepository.IsCourierReceivingTypeAsync(cmd.ReceivingTypeId.Value));

                        RuleFor(x => x.GateInwardDetails)
                            .NotNull().WithMessage($"GateInwardDetails {rule.Error}")
                            .NotEmpty().WithMessage($"GateInwardDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateGateInwardCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));

                        RuleFor(x => x.CourierNumber)
                            .MaximumLength(maxLengthCourierNumber)
                            .WithMessage($"{nameof(CreateGateInwardCommand.CourierNumber)} {rule.Error} {maxLengthCourierNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.CourierNumber));
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

                        RuleFor(x => x.ReceivingTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateGateInwardCommand.ReceivingTypeId)} {rule.Error}")
                            .When(x => x.ReceivingTypeId.HasValue && x.ReceivingTypeId > 0);
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

                    case "GreaterThan":
                        RuleForEach(x => x.GateInwardDetails)
                            .ChildRules(detail =>
                            {
                                // For PO-backed lines (user picked a PO) both PoId and DcQuantity must
                                // be present and > 0 — the downstream GRN tolerance check needs them.
                                // For non-PO lines (manual receipt) all three can be null/absent.
                                detail.RuleFor(d => d.PoId)
                                    .NotNull().WithMessage($"PoId {rule.Error}")
                                    .GreaterThan(0).WithMessage($"PoId {rule.Error}")
                                    .When(d => !string.IsNullOrWhiteSpace(d.ReferenceDocNo));

                                detail.RuleFor(d => d.DcQuantity)
                                    .NotNull().WithMessage($"DcQuantity {rule.Error}")
                                    .GreaterThan(0).WithMessage($"DcQuantity {rule.Error}")
                                    .When(d => d.PoId.HasValue && d.PoId.Value > 0);
                            })
                            .When(x => x.GateInwardDetails != null && x.GateInwardDetails.Count > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
