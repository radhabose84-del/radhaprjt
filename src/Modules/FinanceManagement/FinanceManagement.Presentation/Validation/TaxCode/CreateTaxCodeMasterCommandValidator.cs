using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class CreateTaxCodeMasterCommandValidator : AbstractValidator<CreateTaxCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public CreateTaxCodeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthTaxCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.TaxCodeMaster>("TaxCode") ?? 20;
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
                        RuleFor(x => x.CompanyId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.TaxCode)
                            .NotNull().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}");

                        RuleFor(x => x.TaxName)
                            .NotNull().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxName)} {rule.Error}");

                        RuleFor(x => x.TaxType)
                            .NotNull().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxType)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxType)} {rule.Error}");
                        break;

                    case "TaxCodePattern":
                        RuleFor(x => x.TaxCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TaxCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TaxCode)
                            .MaximumLength(maxLengthTaxCode)
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error} {maxLengthTaxCode} characters.");

                        RuleFor(x => x.TaxName)
                            .MaximumLength(maxLengthTaxName)
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxName)} {rule.Error} {maxLengthTaxName} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ParentTaxCodeId)
                            .MustAsync(async (parentId, ct) => await _queryRepository.TaxCodeExistsAsync(parentId!.Value))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.ParentTaxCodeId)} {rule.Error}")
                            .When(x => x.ParentTaxCodeId.HasValue && x.ParentTaxCodeId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TaxCode)
                            .MustAsync(async (command, taxCode, ct) =>
                                !await _queryRepository.TaxCodeAlreadyExistsAsync(taxCode!, command.CompanyId))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TaxCode) && x.CompanyId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Business rules (AC2-A / AC4-A / component-code link).
            RuleFor(x => x.Direction)
                .NotEmpty()
                .WithMessage("Direction is required for GST/IGST codes.")
                .When(x => x.TaxType == "GST_IN" || x.TaxType == "GST_OUT" || x.TaxType == "IGST");

            RuleFor(x => x.StatutorySection)
                .NotEmpty()
                .WithMessage("Statutory section is required for TDS codes.")
                .When(x => x.TaxType == "TDS");

            RuleFor(x => x.RatePercent)
                .GreaterThan(0)
                .WithMessage("Rate is required for GST/IGST/customs codes.")
                .When(x => x.TaxType == "GST_IN" || x.TaxType == "GST_OUT" || x.TaxType == "IGST" || x.TaxType == "CUSTOMS");

            RuleFor(x => x.RatePercent)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Rate cannot be negative.");

            RuleFor(x => x.ParentTaxCodeId)
                .NotNull()
                .WithMessage("Component children must reference a parent code.")
                .When(x => x.TaxComponent == "CGST" || x.TaxComponent == "SGST" || x.TaxComponent == "IGST" || x.TaxComponent == "CESS");
        }
    }
}
