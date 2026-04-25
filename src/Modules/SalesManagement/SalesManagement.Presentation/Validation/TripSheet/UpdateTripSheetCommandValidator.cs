using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.TripSheet
{
    public class UpdateTripSheetCommandValidator : AbstractValidator<UpdateTripSheetCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITripSheetQueryRepository _queryRepo;

        public UpdateTripSheetCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITripSheetQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthVehicleNo = maxLengthProvider.GetMaxLength<Domain.Entities.TripSheetHeader>("VehicleNo") ?? 20;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.TripSheetHeader>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.VehicleNo)
                            .NotNull().WithMessage($"{nameof(UpdateTripSheetCommand.VehicleNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTripSheetCommand.VehicleNo)} {rule.Error}");

                        RuleFor(x => x.TripDate)
                            .NotEmpty().WithMessage($"{nameof(UpdateTripSheetCommand.TripDate)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNo)
                            .MaximumLength(maxLengthVehicleNo)
                            .WithMessage($"{nameof(UpdateTripSheetCommand.VehicleNo)} {rule.Error} {maxLengthVehicleNo} characters.");

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateTripSheetCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"TripSheet {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateTripSheetCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.DispatchAdviceHeaderId)
                                .GreaterThan(0)
                                .WithMessage($"DispatchAdviceHeaderId {rule.Error}");

                            detail.RuleFor(d => d.SequenceNo)
                                .GreaterThan(0)
                                .WithMessage($"SequenceNo {rule.Error}");
                        }).When(x => x.Details != null && x.Details.Count > 0);
                        break;

                    case "FKColumnDelete":
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.DispatchAdviceHeaderId)
                                .MustAsync(async (id, ct) => await _queryRepo.DispatchExistsAsync(id))
                                .WithMessage($"DispatchAdviceHeaderId {rule.Error}")
                                .When(d => d.DispatchAdviceHeaderId > 0);
                        }).When(x => x.Details != null && x.Details.Count > 0);

                        // Dispatch-not-in-another-trip check (needs parent Id to exclude current trip)
                        RuleForEach(x => x.Details)
                            .MustAsync(async (command, detail, ct) =>
                            {
                                if (detail.DispatchAdviceHeaderId <= 0) return true;
                                return !await _queryRepo.DispatchAlreadyInTripAsync(detail.DispatchAdviceHeaderId, command.Id);
                            })
                            .WithMessage("This dispatch is already assigned to another trip sheet.")
                            .When(x => x.Details != null && x.Details.Count > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
