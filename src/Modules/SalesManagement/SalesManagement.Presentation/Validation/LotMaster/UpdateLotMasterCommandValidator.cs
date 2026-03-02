using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ILotMaster;
using SalesManagement.Application.LotMaster.Commands.UpdateLotMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.LotMaster
{
    public class UpdateLotMasterCommandValidator : AbstractValidator<UpdateLotMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ILotMasterQueryRepository _queryRepository;

        public UpdateLotMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ILotMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.LotMaster>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            // Inline: FK int fields must be > 0
            RuleFor(x => x.LotTypeId).GreaterThan(0).WithMessage("LotTypeId must be greater than zero.");
            RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("StatusId must be greater than zero.");

            // Inline: StartDate cannot be future-dated
            RuleFor(x => x.StartDate)
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage($"{nameof(UpdateLotMasterCommand.StartDate)} cannot be a future date.")
                .When(x => x.StartDate != default);

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"LotMaster {rule.Error}");
                        break;

                    case "NotEmpty":
                        RuleFor(x => x.StartDate)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateLotMasterCommand.StartDate)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateLotMasterCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.LotTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.LotTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateLotMasterCommand.LotTypeId)} {rule.Error}")
                            .When(x => x.LotTypeId > 0);

                        RuleFor(x => x.StatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.StatusExistsAsync(id))
                            .WithMessage($"{nameof(UpdateLotMasterCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateLotMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
