using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAdvice
{
    public class DeleteDispatchAdviceCommandValidator : AbstractValidator<DeleteDispatchAdviceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAdviceQueryRepository _queryRepository;

        public DeleteDispatchAdviceCommandValidator(IDispatchAdviceQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteDispatchAdviceCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"DispatchAdvice {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.HasInvoiceAsync(id))
                            .WithMessage("Deletion not allowed. This Dispatch Advice is already invoiced.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
