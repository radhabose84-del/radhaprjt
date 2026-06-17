using Contracts.Interfaces;
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
        private readonly IIPAddressService _ipAddressService;

        public CreateTaxCodeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITaxCodeQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

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
                        RuleFor(x => x.TaxCode)
                            .NotNull().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}");

                        RuleFor(x => x.TaxName)
                            .NotNull().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxName)} {rule.Error}");

                        RuleFor(x => x.TaxTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxTypeId)} {rule.Error}");
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
                        RuleFor(x => x.TaxTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.TaxTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxTypeId)} {rule.Error}")
                            .When(x => x.TaxTypeId > 0);

                        RuleFor(x => x.TaxComponentId)
                            .MustAsync(async (id, ct) => await _queryRepository.TaxComponentExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxComponentId)} {rule.Error}")
                            .When(x => x.TaxComponentId.HasValue && x.TaxComponentId.Value > 0);

                        RuleFor(x => x.DirectionId)
                            .MustAsync(async (id, ct) => await _queryRepository.DirectionExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.DirectionId)} {rule.Error}")
                            .When(x => x.DirectionId.HasValue && x.DirectionId.Value > 0);

                        RuleFor(x => x.ParentTaxCodeId)
                            .MustAsync(async (parentId, ct) => await _queryRepository.TaxCodeExistsAsync(parentId!.Value))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.ParentTaxCodeId)} {rule.Error}")
                            .When(x => x.ParentTaxCodeId.HasValue && x.ParentTaxCodeId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TaxCode)
                            .MustAsync(async (taxCode, ct) =>
                                !await _queryRepository.TaxCodeAlreadyExistsAsync(taxCode!, _ipAddressService.GetCompanyId() ?? 0))
                            .WithMessage($"{nameof(CreateTaxCodeMasterCommand.TaxCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TaxCode));
                        break;

                    default:
                        break;
                }
            }

            // Business rules (AC2-A / AC4-A / component-code link) — resolve the MiscMaster code
            // for the selected TaxTypeId / TaxComponentId to branch.
            RuleFor(x => x.DirectionId)
                .MustAsync(async (cmd, dirId, ct) =>
                {
                    var type = await _queryRepository.GetMiscCodeAsync(cmd.TaxTypeId);
                    var needsDirection = type is "GST_IN" or "GST_OUT" or "IGST";
                    return !needsDirection || (dirId.HasValue && dirId.Value > 0);
                })
                .WithMessage("Direction is required for GST/IGST codes.")
                .When(x => x.TaxTypeId > 0);

            RuleFor(x => x.StatutorySection)
                .MustAsync(async (cmd, section, ct) =>
                {
                    var type = await _queryRepository.GetMiscCodeAsync(cmd.TaxTypeId);
                    return type != "TDS" || !string.IsNullOrWhiteSpace(section);
                })
                .WithMessage("Statutory section is required for TDS codes.")
                .When(x => x.TaxTypeId > 0);

            RuleFor(x => x.RatePercent)
                .MustAsync(async (cmd, rate, ct) =>
                {
                    var type = await _queryRepository.GetMiscCodeAsync(cmd.TaxTypeId);
                    var needsRate = type is "GST_IN" or "GST_OUT" or "IGST" or "CUSTOMS";
                    return !needsRate || rate > 0;
                })
                .WithMessage("Rate is required for GST/IGST/customs codes.")
                .When(x => x.TaxTypeId > 0);

            RuleFor(x => x.RatePercent)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Rate cannot be negative.");

            RuleFor(x => x.ParentTaxCodeId)
                .MustAsync(async (cmd, parentId, ct) =>
                {
                    if (!cmd.TaxComponentId.HasValue) return true;
                    var component = await _queryRepository.GetMiscCodeAsync(cmd.TaxComponentId.Value);
                    var isComponentChild = component is "CGST" or "SGST" or "IGST" or "CESS";
                    return !isComponentChild || (parentId.HasValue && parentId.Value > 0);
                })
                .WithMessage("Component children must reference a parent code.")
                .When(x => x.TaxComponentId.HasValue);
        }
    }
}
