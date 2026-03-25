using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.VehicleMovementRecord
{
    public class CreateVehicleMovementRecordCommandValidator : AbstractValidator<CreateVehicleMovementRecordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;

        public CreateVehicleMovementRecordCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVehicleMovementRecordQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("VehicleNumber") ?? 20;
            var maxLengthDriverName = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("DriverName") ?? 50;
            var maxLengthDriverLicenseNo = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("DriverLicenseNo") ?? 25;
            var maxLengthDriverMobileNo = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("DriverMobileNo") ?? 10;
            var maxLengthReferenceDocNo = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("ReferenceDocNo") ?? 20;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("Remarks") ?? 250;

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
                        RuleFor(x => x.VehicleNumber)
                            .NotNull().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.VehicleNumber)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.VehicleNumber)} {rule.Error}");

                        RuleFor(x => x.DriverName)
                            .NotNull().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverName)} {rule.Error}");

                        RuleFor(x => x.DriverMobileNo)
                            .NotNull().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverMobileNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverMobileNo)} {rule.Error}");

                        RuleFor(x => x.PurposeOfVisitId)
                            .NotNull().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotNull().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.UnitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVehicleMovementRecordCommand.UnitId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.");

                        RuleFor(x => x.DriverName)
                            .MaximumLength(maxLengthDriverName)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverName)} {rule.Error} {maxLengthDriverName} characters.");

                        RuleFor(x => x.DriverLicenseNo)
                            .MaximumLength(maxLengthDriverLicenseNo)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverLicenseNo)} {rule.Error} {maxLengthDriverLicenseNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverLicenseNo));

                        RuleFor(x => x.ReferenceDocNo)
                            .MaximumLength(maxLengthReferenceDocNo)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.ReferenceDocNo)} {rule.Error} {maxLengthReferenceDocNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceDocNo));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.DriverMobileNo)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.DriverMobileNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverMobileNo));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PurposeOfVisitId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}")
                            .When(x => x.PurposeOfVisitId > 0);

                        RuleFor(x => x.ReferenceDocTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.ReferenceDocTypeId)} {rule.Error}")
                            .When(x => x.ReferenceDocTypeId.HasValue && x.ReferenceDocTypeId > 0);

                        RuleFor(x => x.TransporterId)
                            .MustAsync(async (id, ct) => await _queryRepository.TransporterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.TransporterId)} {rule.Error}")
                            .When(x => x.TransporterId.HasValue && x.TransporterId > 0);

                        RuleFor(x => x.UnitId)
                            .MustAsync(async (id, ct) => await _queryRepository.UnitExistsAsync(id))
                            .WithMessage($"{nameof(CreateVehicleMovementRecordCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);
                        break;

                    case "AlreadyExists":
                        // R1: No open VMR for same vehicle
                        RuleFor(x => x.VehicleNumber)
                            .MustAsync(async (vehicleNumber, ct) =>
                                !await _queryRepository.HasOpenVMRForVehicleAsync(vehicleNumber!))
                            .WithMessage("An open movement exists for this vehicle.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
