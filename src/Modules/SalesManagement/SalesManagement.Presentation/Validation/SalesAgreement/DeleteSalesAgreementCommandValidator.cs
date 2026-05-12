using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesAgreement
{
    public class DeleteSalesAgreementCommandValidator : AbstractValidator<DeleteSalesAgreementCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesAgreementQueryRepository _queryRepository;

        public DeleteSalesAgreementCommandValidator(ISalesAgreementQueryRepository queryRepository)
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
                            .NotEmpty()
                            .WithMessage($"Id {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Agreement {rule.Error}")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
