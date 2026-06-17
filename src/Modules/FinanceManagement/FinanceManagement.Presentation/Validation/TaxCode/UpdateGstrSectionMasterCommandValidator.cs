using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class UpdateGstrSectionMasterCommandValidator : AbstractValidator<UpdateGstrSectionMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;

        public UpdateGstrSectionMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGstrSectionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.SectionName)
                            .NotNull().WithMessage($"{nameof(UpdateGstrSectionMasterCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateGstrSectionMasterCommand.SectionName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateGstrSectionMasterCommand.SectionName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.SectionNotFoundAsync(id))
                            .WithMessage($"GSTR section {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateGstrSectionMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
