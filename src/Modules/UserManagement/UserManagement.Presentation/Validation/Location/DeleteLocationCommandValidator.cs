using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Command.DeleteLocation;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Location
{
    public class DeleteLocationCommandValidator : AbstractValidator<DeleteLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ILocationQueryRepository _locationQueryRepository;

        public DeleteLocationCommandValidator(ILocationQueryRepository locationQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _locationQueryRepository = locationQueryRepository;
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
                            .WithMessage($"{nameof(DeleteLocationCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => !await _locationQueryRepository.NotFoundAsync(id))
                            .WithMessage($"Location {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
