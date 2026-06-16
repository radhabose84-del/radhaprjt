using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMapping;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class UpdateGstrSectionMappingCommandValidator : AbstractValidator<UpdateGstrSectionMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public UpdateGstrSectionMappingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.SectionName)
                            .NotNull().WithMessage($"{nameof(UpdateGstrSectionMappingCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateGstrSectionMappingCommand.SectionName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthSectionName)
                            .WithMessage($"{nameof(UpdateGstrSectionMappingCommand.SectionName)} {rule.Error} {maxLengthSectionName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.GstrMappingNotFoundAsync(id))
                            .WithMessage($"GSTR section mapping {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateGstrSectionMappingCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
