using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class UpdateTaxCodeMasterCommandValidator : AbstractValidator<UpdateTaxCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public UpdateTaxCodeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthTaxName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.TaxCodeMaster>("TaxName") ?? 150;

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
                        RuleFor(x => x.TaxName)
                            .NotNull().WithMessage($"{nameof(UpdateTaxCodeMasterCommand.TaxName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTaxCodeMasterCommand.TaxName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TaxName)
                            .MaximumLength(maxLengthTaxName)
                            .WithMessage($"{nameof(UpdateTaxCodeMasterCommand.TaxName)} {rule.Error} {maxLengthTaxName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.TaxCodeNotFoundAsync(id))
                            .WithMessage($"Tax Code {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateTaxCodeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
