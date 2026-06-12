using UserManagement.Application.IconMaster.Commands.DeleteIconMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.IconMaster
{
    public class DeleteIconMasterCommandValidator : AbstractValidator<DeleteIconMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public DeleteIconMasterCommandValidator()
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
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
                            .WithMessage($"{nameof(DeleteIconMasterCommand.Id)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
