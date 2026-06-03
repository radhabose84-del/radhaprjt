using FluentValidation;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeAllocation
{
    public class UpdateBarcodeAllocationCommandValidator : AbstractValidator<UpdateBarcodeAllocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public UpdateBarcodeAllocationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IBarcodeAllocationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var employeeNoMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.BarcodeAllocation>("EmployeeNo") ?? 50;
            var employeeNameMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.BarcodeAllocation>("EmployeeName") ?? 150;
            var remarksMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.BarcodeAllocation>("Remarks") ?? 250;

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
                        RuleFor(x => x.BarcodeSeriesId)
                            .NotEmpty().WithMessage($"{nameof(UpdateBarcodeAllocationCommand.BarcodeSeriesId)} {rule.Error}");
                        RuleFor(x => x.EmployeeNo)
                            .NotNull().WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeNo)} {rule.Error}");
                        RuleFor(x => x.EmployeeName)
                            .NotNull().WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeName)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.BarcodeFrom)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateBarcodeAllocationCommand.BarcodeFrom)} {rule.Error}");
                        RuleFor(x => x.BarcodeTo)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateBarcodeAllocationCommand.BarcodeTo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.EmployeeNo)
                            .MaximumLength(employeeNoMax).WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeNo)} {rule.Error} {employeeNoMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeNo));
                        RuleFor(x => x.EmployeeName)
                            .MaximumLength(employeeNameMax).WithMessage($"{nameof(UpdateBarcodeAllocationCommand.EmployeeName)} {rule.Error} {employeeNameMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeName));
                        RuleFor(x => x.Remarks)
                            .MaximumLength(remarksMax).WithMessage($"{nameof(UpdateBarcodeAllocationCommand.Remarks)} {rule.Error} {remarksMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Barcode Allocation")
                            .WithMessage($"Barcode allocation {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.BarcodeSeriesId)
                            .MustAsync(async (seriesId, ct) => await _queryRepository.SeriesExistsAsync(seriesId))
                            .WithMessage($"{nameof(UpdateBarcodeAllocationCommand.BarcodeSeriesId)} {rule.Error}")
                            .When(x => x.BarcodeSeriesId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                                !await _queryRepository.RangeOverlapsInSeriesAsync(command.BarcodeSeriesId, command.BarcodeFrom, command.BarcodeTo, command.Id))
                            .WithMessage($"Barcode range {rule.Error}")
                            .WithName("Barcode Range")
                            .When(x => x.BarcodeSeriesId > 0 && x.BarcodeFrom > 0 && x.BarcodeTo > 0 && x.BarcodeTo >= x.BarcodeFrom);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateBarcodeAllocationCommand.IsActive)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsUsedAsync(id))
                            .WithMessage("This allocation has used barcodes and cannot be modified.");
                        break;

                    default:
                        break;
                }
            }

            // Cross-field: End must be greater than Start (R3).
            RuleFor(x => x.BarcodeTo)
                .GreaterThan(x => x.BarcodeFrom)
                .WithMessage("Barcode To must be greater than Barcode From.")
                .When(x => x.BarcodeFrom > 0 && x.BarcodeTo > 0);

            // R2 — range must sit within the selected series' Start..End.
            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                    await _queryRepository.IsWithinSeriesRangeAsync(command.BarcodeSeriesId, command.BarcodeFrom, command.BarcodeTo))
                .WithMessage("Barcode range must be within the selected series range.")
                .WithName("Barcode Range")
                .When(x => x.BarcodeSeriesId > 0 && x.BarcodeFrom > 0 && x.BarcodeTo > 0 && x.BarcodeTo >= x.BarcodeFrom);
        }
    }
}
