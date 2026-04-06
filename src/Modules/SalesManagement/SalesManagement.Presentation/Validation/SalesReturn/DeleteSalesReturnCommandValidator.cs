using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesReturn
{
    public class DeleteSalesReturnCommandValidator : AbstractValidator<DeleteSalesReturnCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesReturnQueryRepository _queryRepo;

        public DeleteSalesReturnCommandValidator(ISalesReturnQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteSalesReturnCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Sales Return {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
