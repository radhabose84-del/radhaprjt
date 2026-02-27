using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.DeleteSalesLead;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesLead
{
    public class DeleteSalesLeadCommandValidator : AbstractValidator<DeleteSalesLeadCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesLeadQueryRepository _queryRepository;

        public DeleteSalesLeadCommandValidator(ISalesLeadQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesLeadCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Lead {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
