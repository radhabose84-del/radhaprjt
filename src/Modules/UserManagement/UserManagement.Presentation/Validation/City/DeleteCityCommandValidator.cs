using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.Common.Interfaces.ICity;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.City
{
    public class DeleteCityCommandValidator : AbstractValidator<DeleteCityCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICityQueryRepository _cityQueryRepository;

        public DeleteCityCommandValidator(ICityQueryRepository cityQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _cityQueryRepository = cityQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
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
                            .WithMessage($"{nameof(DeleteCityCommand.Id)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                !await _cityQueryRepository.SoftDeleteValidation(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}