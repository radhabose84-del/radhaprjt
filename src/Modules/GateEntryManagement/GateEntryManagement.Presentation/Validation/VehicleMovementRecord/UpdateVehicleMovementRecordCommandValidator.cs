using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.VehicleMovementRecord
{
    public class UpdateVehicleMovementRecordCommandValidator : AbstractValidator<UpdateVehicleMovementRecordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;

        public UpdateVehicleMovementRecordCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVehicleMovementRecordQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("VehicleNumber") ?? 20;
            var maxLengthDriverName = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("DriverName") ?? 50;
            var maxLengthDriverLicenseNo = maxLengthProvider.GetMaxLength<Domain.Entities.VehicleMovementRecord>("DriverLicenseNo") ?? 25;
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
                        // VehicleNumber, DriverName, DriverMobileNo are now optional —
                        // user fills them only when receiving type = Vehicle (UI-driven).
                        RuleFor(x => x.PurposeOfVisitId)
                            .NotNull().WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.");

                        RuleFor(x => x.DriverName)
                            .MaximumLength(maxLengthDriverName)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.DriverName)} {rule.Error} {maxLengthDriverName} characters.");

                        RuleFor(x => x.DriverLicenseNo)
                            .MaximumLength(maxLengthDriverLicenseNo)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.DriverLicenseNo)} {rule.Error} {maxLengthDriverLicenseNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverLicenseNo));

                        RuleFor(x => x.ReferenceDocNo)
                            .MaximumLength(maxLengthReferenceDocNo)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.ReferenceDocNo)} {rule.Error} {maxLengthReferenceDocNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceDocNo));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.DriverMobileNo)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.DriverMobileNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverMobileNo));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Vehicle Movement Record {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PurposeOfVisitId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.PurposeOfVisitId)} {rule.Error}")
                            .When(x => x.PurposeOfVisitId > 0);

                        RuleFor(x => x.ReferenceDocTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.ReferenceDocTypeId)} {rule.Error}")
                            .When(x => x.ReferenceDocTypeId.HasValue && x.ReferenceDocTypeId > 0);

                        RuleFor(x => x.TransporterId)
                            .MustAsync(async (id, ct) => await _queryRepository.TransporterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.TransporterId)} {rule.Error}")
                            .When(x => x.TransporterId.HasValue && x.TransporterId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateVehicleMovementRecordCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
