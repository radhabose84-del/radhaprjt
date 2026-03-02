using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesEnquiry
{
    public class DeleteSalesEnquiryCommandValidator : AbstractValidator<DeleteSalesEnquiryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesEnquiryQueryRepository _queryRepository;

        public DeleteSalesEnquiryCommandValidator(ISalesEnquiryQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesEnquiryCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesEnquiry {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
