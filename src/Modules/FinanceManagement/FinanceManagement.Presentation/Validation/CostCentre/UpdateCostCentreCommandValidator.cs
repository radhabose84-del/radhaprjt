using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CostCentre
{
    public class UpdateCostCentreCommandValidator : AbstractValidator<UpdateCostCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCentreQueryRepository _queryRepository;

        public UpdateCostCentreCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICostCentreQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.CostCentre>("CostCentreName") ?? 100;

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
                        RuleFor(x => x.CostCentreName)
                            .NotNull().WithMessage($"{nameof(UpdateCostCentreCommand.CostCentreName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateCostCentreCommand.CostCentreName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CostCentreName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateCostCentreCommand.CostCentreName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Cost Centre {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateCostCentreCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
