using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.GatePass
{
    public class CreateGatePassCommandValidator : AbstractValidator<CreateGatePassCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGatePassQueryRepository _queryRepository;

        public CreateGatePassCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGatePassQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber = maxLengthProvider.GetMaxLength<Domain.Entities.GatePassHdr>("VehicleNumber") ?? 20;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.GatePassHdr>("Remarks") ?? 500;

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
                        RuleFor(x => x.VehicleMovementRecordId)
                            .NotNull().WithMessage($"{nameof(CreateGatePassCommand.VehicleMovementRecordId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGatePassCommand.VehicleMovementRecordId)} {rule.Error}");

                        RuleFor(x => x.VehicleNumber)
                            .NotNull().WithMessage($"{nameof(CreateGatePassCommand.VehicleNumber)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGatePassCommand.VehicleNumber)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotNull().WithMessage($"{nameof(CreateGatePassCommand.UnitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGatePassCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.GatePassDetails)
                            .NotNull().WithMessage($"GatePassDetails {rule.Error}")
                            .NotEmpty().WithMessage($"GatePassDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(CreateGatePassCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.");

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateGatePassCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.VehicleMovementRecordId)
                            .MustAsync(async (id, ct) => await _queryRepository.VehicleMovementRecordExistsAsync(id))
                            .WithMessage($"{nameof(CreateGatePassCommand.VehicleMovementRecordId)} {rule.Error}")
                            .When(x => x.VehicleMovementRecordId > 0);

                        RuleFor(x => x.UnitId)
                            .MustAsync(async (id, ct) => await _queryRepository.UnitExistsAsync(id))
                            .WithMessage($"{nameof(CreateGatePassCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.GatePassDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.DocTypeId)
                                    .GreaterThan(0)
                                    .WithMessage($"DocTypeId {rule.Error}");

                                detail.RuleFor(d => d.TotalQty)
                                    .GreaterThan(0)
                                    .WithMessage($"TotalQty {rule.Error}");
                            })
                            .When(x => x.GatePassDetails != null && x.GatePassDetails.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.GrossWeight!.Value)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateGatePassCommand.GrossWeight)} {rule.Error}")
                            .When(x => x.GrossWeight.HasValue);

                        RuleFor(x => x.TareWeight!.Value)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateGatePassCommand.TareWeight)} {rule.Error}")
                            .When(x => x.TareWeight.HasValue);

                        RuleFor(x => x.NetWeight!.Value)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateGatePassCommand.NetWeight)} {rule.Error}")
                            .When(x => x.NetWeight.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
