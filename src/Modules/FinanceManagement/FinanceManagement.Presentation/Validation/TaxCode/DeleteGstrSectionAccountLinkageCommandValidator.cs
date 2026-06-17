using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionAccountLinkage;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class DeleteGstrSectionAccountLinkageCommandValidator : AbstractValidator<DeleteGstrSectionAccountLinkageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGstrSectionQueryRepository _queryRepository;

        public DeleteGstrSectionAccountLinkageCommandValidator(IGstrSectionQueryRepository queryRepository)
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
                            .NotEmpty().WithMessage($"{nameof(DeleteGstrSectionAccountLinkageCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.LinkageNotFoundAsync(id))
                            .WithMessage($"GSTR section-account mapping {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
