using FluentValidation;
using PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeSeries
{
    public class DeleteBarcodeSeriesCommandValidator : AbstractValidator<DeleteBarcodeSeriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeSeriesQueryRepository _queryRepository;

        public DeleteBarcodeSeriesCommandValidator(IBarcodeSeriesQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteBarcodeSeriesCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Barcode Series")
                            .WithMessage($"Barcode series {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsAllocatedAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
