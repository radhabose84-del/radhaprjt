using FluentValidation;
using PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeSeries
{
    public class CreateBarcodeSeriesCommandValidator : AbstractValidator<CreateBarcodeSeriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeSeriesQueryRepository _queryRepository;

        public CreateBarcodeSeriesCommandValidator(
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
                            .WithMessage($"{nameof(CreateBarcodeSeriesCommand.PrefixId)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.BarcodeStartNumber)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateBarcodeSeriesCommand.BarcodeStartNumber)} {rule.Error}");
                        RuleFor(x => x.BarcodeEndNumber)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateBarcodeSeriesCommand.BarcodeEndNumber)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(remarksMaxLength)
                            .WithMessage($"{nameof(CreateBarcodeSeriesCommand.Remarks)} {rule.Error} {remarksMaxLength} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PrefixId)
                            .MustAsync(async (prefixId, ct) => await _queryRepository.IsValidPrefixAsync(prefixId))
                            .WithMessage($"{nameof(CreateBarcodeSeriesCommand.PrefixId)} {rule.Error}")
                            .When(x => x.PrefixId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                                !await _queryRepository.RangeOverlapsAsync(command.BarcodeStartNumber, command.BarcodeEndNumber))
                            .WithMessage($"Barcode range {rule.Error}")
                            .WithName("Barcode Range")
                            .When(x => x.BarcodeStartNumber > 0 && x.BarcodeEndNumber > 0 && x.BarcodeEndNumber >= x.BarcodeStartNumber);
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
