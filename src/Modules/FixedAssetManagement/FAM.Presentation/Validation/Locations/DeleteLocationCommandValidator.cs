using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Command.DeleteLocation;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.Locations
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
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _locationQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
