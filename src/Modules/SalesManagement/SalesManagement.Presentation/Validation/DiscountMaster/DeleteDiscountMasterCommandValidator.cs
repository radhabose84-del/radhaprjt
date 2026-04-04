using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DiscountMaster
{
    public class DeleteDiscountMasterCommandValidator : AbstractValidator<DeleteDiscountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDiscountMasterQueryRepository _queryRepository;

        public DeleteDiscountMasterCommandValidator(IDiscountMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteDiscountMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"DiscountMaster {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
