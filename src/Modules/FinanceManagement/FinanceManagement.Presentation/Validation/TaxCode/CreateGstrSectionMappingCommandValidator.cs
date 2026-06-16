using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMapping;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class CreateGstrSectionMappingCommandValidator : AbstractValidator<CreateGstrSectionMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public CreateGstrSectionMappingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthSectionCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GstrSectionMapping>("SectionCode") ?? 20;
            var maxLengthSectionName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GstrSectionMapping>("SectionName") ?? 150;

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
                            .GreaterThan(0).WithMessage($"{nameof(CreateGstrSectionMappingCommand.CompanyId)} {rule.Error}");
                        RuleFor(x => x.GstrType)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMappingCommand.GstrType)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMappingCommand.GstrType)} {rule.Error}");
                        RuleFor(x => x.SectionCode)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionCode)} {rule.Error}");
                        RuleFor(x => x.SectionName)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionName)} {rule.Error}");
                        RuleFor(x => x.AccountRangeFrom)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMappingCommand.AccountRangeFrom)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMappingCommand.AccountRangeFrom)} {rule.Error}");
                        RuleFor(x => x.AccountRangeTo)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMappingCommand.AccountRangeTo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMappingCommand.AccountRangeTo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionCode)
                            .MaximumLength(maxLengthSectionCode)
                            .WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionCode)} {rule.Error} {maxLengthSectionCode} characters.");
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthSectionName)
                            .WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionName)} {rule.Error} {maxLengthSectionName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SectionCode)
                            .MustAsync(async (command, sectionCode, ct) =>
                                !await _queryRepository.GstrMappingAlreadyExistsAsync(command.CompanyId, command.GstrType!, sectionCode!))
                            .WithMessage($"{nameof(CreateGstrSectionMappingCommand.SectionCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GstrType) && !string.IsNullOrWhiteSpace(x.SectionCode) && x.CompanyId > 0);
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.GstrType)
                .Must(v => v == "GSTR1" || v == "GSTR3B")
                .WithMessage("GstrType must be GSTR1 or GSTR3B.")
                .When(x => !string.IsNullOrWhiteSpace(x.GstrType));
        }
    }
}
