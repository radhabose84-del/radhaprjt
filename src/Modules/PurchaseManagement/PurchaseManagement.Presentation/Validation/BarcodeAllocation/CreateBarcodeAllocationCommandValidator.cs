using FluentValidation;
using PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.BarcodeAllocation
{
    public class CreateBarcodeAllocationCommandValidator : AbstractValidator<CreateBarcodeAllocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public CreateBarcodeAllocationCommandValidator(
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
                            .NotEmpty().WithMessage($"{nameof(CreateBarcodeAllocationCommand.BarcodeSeriesId)} {rule.Error}");
                        RuleFor(x => x.AllocationDate)
                            .NotEmpty().WithMessage($"{nameof(CreateBarcodeAllocationCommand.AllocationDate)} {rule.Error}");
                        RuleFor(x => x.EmployeeNo)
                            .NotNull().WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeNo)} {rule.Error}");
                        RuleFor(x => x.EmployeeName)
                            .NotNull().WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeName)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.BarcodeFrom)
                            .GreaterThan(0).WithMessage($"{nameof(CreateBarcodeAllocationCommand.BarcodeFrom)} {rule.Error}");
                        RuleFor(x => x.BarcodeTo)
                            .GreaterThan(0).WithMessage($"{nameof(CreateBarcodeAllocationCommand.BarcodeTo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.EmployeeNo)
                            .MaximumLength(employeeNoMax).WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeNo)} {rule.Error} {employeeNoMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeNo));
                        RuleFor(x => x.EmployeeName)
                            .MaximumLength(employeeNameMax).WithMessage($"{nameof(CreateBarcodeAllocationCommand.EmployeeName)} {rule.Error} {employeeNameMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeName));
                        RuleFor(x => x.Remarks)
                            .MaximumLength(remarksMax).WithMessage($"{nameof(CreateBarcodeAllocationCommand.Remarks)} {rule.Error} {remarksMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.BarcodeSeriesId)
                            .MustAsync(async (seriesId, ct) => await _queryRepository.SeriesExistsAsync(seriesId))
                            .WithMessage($"{nameof(CreateBarcodeAllocationCommand.BarcodeSeriesId)} {rule.Error}")
                            .When(x => x.BarcodeSeriesId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                                !await _queryRepository.RangeOverlapsInSeriesAsync(command.BarcodeSeriesId, command.BarcodeFrom, command.BarcodeTo))
                            .WithMessage($"Barcode range {rule.Error}")
                            .WithName("Barcode Range")
                            .When(x => x.BarcodeSeriesId > 0 && x.BarcodeFrom > 0 && x.BarcodeTo > 0 && x.BarcodeTo >= x.BarcodeFrom);
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
