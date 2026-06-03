using FluentValidation;
using PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeSeries
{
    public class UpdateBarcodeSeriesCommandValidator : AbstractValidator<UpdateBarcodeSeriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeSeriesQueryRepository _queryRepository;

        public UpdateBarcodeSeriesCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IBarcodeSeriesQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var remarksMaxLength = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.BarcodeSeries>("Remarks") ?? 250;

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
                        RuleFor(x => x.PrefixId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.PrefixId)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.BarcodeStartNumber)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.BarcodeStartNumber)} {rule.Error}");
                        RuleFor(x => x.BarcodeEndNumber)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.BarcodeEndNumber)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(remarksMaxLength)
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.Remarks)} {rule.Error} {remarksMaxLength} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Barcode Series")
                            .WithMessage($"Barcode series {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PrefixId)
                            .MustAsync(async (prefixId, ct) => await _queryRepository.IsValidPrefixAsync(prefixId))
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.PrefixId)} {rule.Error}")
                            .When(x => x.PrefixId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                                !await _queryRepository.RangeOverlapsAsync(command.BarcodeStartNumber, command.BarcodeEndNumber, command.Id))
                            .WithMessage($"Barcode range {rule.Error}")
                            .WithName("Barcode Range")
                            .When(x => x.BarcodeStartNumber > 0 && x.BarcodeEndNumber > 0 && x.BarcodeEndNumber >= x.BarcodeStartNumber);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateBarcodeSeriesCommand.IsActive)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsAllocatedAsync(id))
                            .WithMessage("This barcode series has allocated barcodes and cannot be modified.");
                        break;

                    default:
                        break;
                }
            }

            // Cross-field: End must be greater than Start (R2).
            RuleFor(x => x.BarcodeEndNumber)
                .GreaterThan(x => x.BarcodeStartNumber)
                .WithMessage("Barcode End Number must be greater than Barcode Start Number.")
                .When(x => x.BarcodeStartNumber > 0 && x.BarcodeEndNumber > 0);
        }
    }
}
