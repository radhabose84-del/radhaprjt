using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionAccountLinkage;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class UpdateGstrSectionAccountLinkageCommandValidator : AbstractValidator<UpdateGstrSectionAccountLinkageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;

        public UpdateGstrSectionAccountLinkageCommandValidator(IGstrSectionQueryRepository queryRepository)
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
                        RuleFor(x => x.SectionMasterId)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.SectionMasterId)} {rule.Error}");
                        RuleFor(x => x.AccountRangeFrom)
                            .NotEmpty().WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.AccountRangeFrom)} {rule.Error}");
                        RuleFor(x => x.AccountRangeTo)
                            .NotEmpty().WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.AccountRangeTo)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.LinkageNotFoundAsync(id))
                            .WithMessage($"GSTR section-account mapping {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SectionMasterId)
                            .MustAsync(async (id, ct) => await _queryRepository.SectionExistsAsync(id))
                            .WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.SectionMasterId)} {rule.Error}")
                            .When(x => x.SectionMasterId > 0);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.ExpectedValue)
                            .GreaterThanOrEqualTo(0).WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.ExpectedValue)} {rule.Error}");
                        RuleFor(x => x.TolerancePercent)
                            .InclusiveBetween(0m, 100m).WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.TolerancePercent)} must be between 0 and 100.");
                        RuleFor(x => x.DerivedValue!.Value)
                            .GreaterThanOrEqualTo(0).WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.DerivedValue)} {rule.Error}")
                            .When(x => x.DerivedValue.HasValue);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateGstrSectionAccountLinkageCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
