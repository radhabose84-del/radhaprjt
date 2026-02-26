using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrganisation
{
    public class DeleteSalesOrganisationCommandValidator : AbstractValidator<DeleteSalesOrganisationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public DeleteSalesOrganisationCommandValidator(ISalesOrganisationQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesOrganisationCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Organisation {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
