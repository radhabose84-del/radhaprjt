using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class DeleteGstrSectionMasterCommandValidator : AbstractValidator<DeleteGstrSectionMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;

        public DeleteGstrSectionMasterCommandValidator(IGstrSectionQueryRepository queryRepository)
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
                            .NotEmpty().WithMessage($"{nameof(DeleteGstrSectionMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SectionNotFoundAsync(id))
                            .WithMessage($"GSTR section {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SectionHasLinkagesAsync(id))
                            .WithMessage("This section is linked with account mappings. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
