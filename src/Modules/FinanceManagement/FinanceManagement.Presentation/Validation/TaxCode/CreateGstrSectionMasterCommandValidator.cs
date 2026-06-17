using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class CreateGstrSectionMasterCommandValidator : AbstractValidator<CreateGstrSectionMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateGstrSectionMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGstrSectionQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GstrSectionMaster>("SectionCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GstrSectionMaster>("SectionName") ?? 200;

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
                        RuleFor(x => x.SectionCode)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionCode)} {rule.Error}");

                        RuleFor(x => x.SectionName)
                            .NotNull().WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionName)} {rule.Error}");

                        RuleFor(x => x.ReportTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateGstrSectionMasterCommand.ReportTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionCode)} {rule.Error} {maxLengthCode} characters.");
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ReportTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.ReportTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateGstrSectionMasterCommand.ReportTypeId)} {rule.Error}")
                            .When(x => x.ReportTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SectionCode)
                            .MustAsync(async (cmd, code, ct) =>
                                !await _queryRepository.SectionAlreadyExistsAsync(
                                    _ipAddressService.GetCompanyId() ?? 0, cmd.ReportTypeId, code!))
                            .WithMessage($"{nameof(CreateGstrSectionMasterCommand.SectionCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SectionCode) && x.ReportTypeId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
