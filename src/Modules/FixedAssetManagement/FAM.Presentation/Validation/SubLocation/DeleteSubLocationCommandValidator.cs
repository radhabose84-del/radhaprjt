using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Command.DeleteAubLocation;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.SubLocation
{
    public class DeleteSubLocationCommandValidator : AbstractValidator<DeleteSubLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISubLocationQueryRepository _subLocationQueryRepository;
        public DeleteSubLocationCommandValidator(ISubLocationQueryRepository subLocationQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _subLocationQueryRepository = subLocationQueryRepository;

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
                            .WithMessage($"{nameof(DeleteSubLocationCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _subLocationQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
