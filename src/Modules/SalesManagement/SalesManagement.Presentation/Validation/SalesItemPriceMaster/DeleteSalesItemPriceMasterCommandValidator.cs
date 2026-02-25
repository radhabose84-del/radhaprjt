#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesItemPriceMaster
{
    public class DeleteSalesItemPriceMasterCommandValidator : AbstractValidator<DeleteSalesItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;

        public DeleteSalesItemPriceMasterCommandValidator(ISalesItemPriceMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesItemPriceMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Item Price Master {rule.Error}");
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
