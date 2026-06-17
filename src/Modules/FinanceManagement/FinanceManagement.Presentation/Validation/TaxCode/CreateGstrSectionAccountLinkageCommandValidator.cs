using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class CreateGstrSectionAccountLinkageCommandValidator : AbstractValidator<CreateGstrSectionAccountLinkageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;

        public CreateGstrSectionAccountLinkageCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGstrSectionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRange = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GstrSectionAccountLinkage>("AccountRangeFrom") ?? 20;

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
                        RuleFor(x => x.SectionMasterId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.SectionMasterId)} {rule.Error}");
                        RuleFor(x => x.AccountRangeFrom)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeFrom)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeFrom)} {rule.Error}");
                        RuleFor(x => x.AccountRangeTo)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeTo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeTo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.AccountRangeFrom)
                            .MaximumLength(maxLengthRange)
                            .WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeFrom)} {rule.Error} {maxLengthRange} characters.");
                        RuleFor(x => x.AccountRangeTo)
                            .MaximumLength(maxLengthRange)
                            .WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.AccountRangeTo)} {rule.Error} {maxLengthRange} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SectionMasterId)
                            .MustAsync(async (id, ct) => await _queryRepository.SectionExistsAsync(id))
                            .WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.SectionMasterId)} {rule.Error}")
                            .When(x => x.SectionMasterId > 0);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.ExpectedValue)
                            .GreaterThanOrEqualTo(0).WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.ExpectedValue)} {rule.Error}");
                        RuleFor(x => x.TolerancePercent)
                            .InclusiveBetween(0m, 100m).WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.TolerancePercent)} must be between 0 and 100.");
                        RuleFor(x => x.DerivedValue!.Value)
                            .GreaterThanOrEqualTo(0).WithMessage($"{nameof(CreateGstrSectionAccountLinkageCommand.DerivedValue)} {rule.Error}")
                            .When(x => x.DerivedValue.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
