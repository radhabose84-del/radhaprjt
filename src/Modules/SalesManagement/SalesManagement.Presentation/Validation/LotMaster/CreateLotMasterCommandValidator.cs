using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ILotMaster;
using SalesManagement.Application.LotMaster.Commands.CreateLotMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.LotMaster
{
    public class CreateLotMasterCommandValidator : AbstractValidator<CreateLotMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ILotMasterQueryRepository _queryRepository;

        public CreateLotMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ILotMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode        = maxLengthProvider.GetMaxLength<Domain.Entities.LotMaster>("LotCode")      ?? 20;
            var maxLengthBatchNumber = maxLengthProvider.GetMaxLength<Domain.Entities.LotMaster>("BatchNumber")  ?? 50;
            var maxLengthRemarks     = maxLengthProvider.GetMaxLength<Domain.Entities.LotMaster>("Remarks")      ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            // Inline: FK int fields must be > 0
            RuleFor(x => x.LotTypeId).GreaterThan(0).WithMessage("LotTypeId must be greater than zero.");
            RuleFor(x => x.ItemId).GreaterThan(0).WithMessage("ItemId must be greater than zero.");
            RuleFor(x => x.UnitId).GreaterThan(0).WithMessage("UnitId must be greater than zero.");
            RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("StatusId must be greater than zero.");

            // Inline: StartDate cannot be future-dated
            RuleFor(x => x.StartDate)
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage($"{nameof(CreateLotMasterCommand.StartDate)} cannot be a future date.")
                .When(x => x.StartDate != default);

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.LotCode)
                            .NotNull().WithMessage($"{nameof(CreateLotMasterCommand.LotCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateLotMasterCommand.LotCode)} {rule.Error}");

                        RuleFor(x => x.BatchNumber)
                            .NotNull().WithMessage($"{nameof(CreateLotMasterCommand.BatchNumber)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateLotMasterCommand.BatchNumber)} {rule.Error}");

                        RuleFor(x => x.StartDate)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(CreateLotMasterCommand.StartDate)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.LotCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateLotMasterCommand.LotCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.LotCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.LotCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateLotMasterCommand.LotCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.BatchNumber)
                            .MaximumLength(maxLengthBatchNumber)
                            .WithMessage($"{nameof(CreateLotMasterCommand.BatchNumber)} {rule.Error} {maxLengthBatchNumber} characters.");

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateLotMasterCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.LotCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateLotMasterCommand.LotCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.LotCode));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.LotTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.LotTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateLotMasterCommand.LotTypeId)} {rule.Error}")
                            .When(x => x.LotTypeId > 0);

                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateLotMasterCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        RuleFor(x => x.UnitId)
                            .MustAsync(async (id, ct) => await _queryRepository.UnitExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateLotMasterCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);

                        RuleFor(x => x.StatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.StatusExistsAsync(id))
                            .WithMessage($"{nameof(CreateLotMasterCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
