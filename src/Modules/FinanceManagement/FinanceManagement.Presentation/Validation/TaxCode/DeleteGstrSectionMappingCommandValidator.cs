using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMapping;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class DeleteGstrSectionMappingCommandValidator : AbstractValidator<DeleteGstrSectionMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public DeleteGstrSectionMappingCommandValidator(ITaxCodeQueryRepository queryRepository)
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
                        RuleFor(x => x.Id)
                            .NotEmpty().WithMessage($"{nameof(DeleteGstrSectionMappingCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.GstrMappingNotFoundAsync(id))
                            .WithMessage($"GSTR section mapping {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
