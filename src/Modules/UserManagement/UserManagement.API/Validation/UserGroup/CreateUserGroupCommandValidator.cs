using FluentValidation;
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.API.Validation.Common;
using Shared.Validation.Common;


namespace UserManagement.API.Validation.UserGroup
{
    public class CreateUserGroupCommandValidator  : AbstractValidator<CreateUserGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateUserGroupCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider
            var groupCodeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserGroup>("GroupCode") ?? 5;
            var groupNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserGroup>("GroupName") ?? 50;

            // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.GroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUserGroupCommand.GroupName)} {rule.Error}");
                        RuleFor(x => x.GroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUserGroupCommand.GroupCode)} {rule.Error}");
                            break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.GroupName)
                            .MaximumLength(groupNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUserGroupCommand.GroupName)} {rule.Error} {groupNameMaxLength}");
                        RuleFor(x => x.GroupCode)
                            .MaximumLength(groupCodeMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUserGroupCommand.GroupCode)} {rule.Error} {groupCodeMaxLength}");
                            break;                 
                    default:                        
                        break;
                }
            }
        }
    }
}