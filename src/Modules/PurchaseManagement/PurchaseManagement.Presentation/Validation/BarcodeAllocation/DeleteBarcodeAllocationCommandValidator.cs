using FluentValidation;
using PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeAllocation
{
    public class DeleteBarcodeAllocationCommandValidator : AbstractValidator<DeleteBarcodeAllocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public DeleteBarcodeAllocationCommandValidator(IBarcodeAllocationQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteBarcodeAllocationCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Barcode Allocation")
                            .WithMessage($"Barcode allocation {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsUsedAsync(id))
                            .WithMessage("This allocation has used barcodes and cannot be deleted.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
