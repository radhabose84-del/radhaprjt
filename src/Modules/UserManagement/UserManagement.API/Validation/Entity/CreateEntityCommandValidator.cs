using FluentValidation;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Entity
{
    public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
    {
         private readonly List<ValidationRule> _validationRules;
         
         public CreateEntityCommandValidator(MaxLengthProvider maxLengthProvider)
         {
            var EntityNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Entity>("EntityName") ?? 100;
            var EntityDescriptionMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Entity>("EntityDescription") ?? 250;
            var AddressMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Entity>("Address") ?? 200;
            var PhoneMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Entity>("Phone") ?? 40;
            var EmailMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Entity>("Email") ?? 200;

            // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
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
                        RuleFor(x => x.EntityName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEntityCommand.EntityName)} {rule.Error}");
                        // RuleFor(x => x.EntityDescription)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateEntityCommand.EntityDescription)} {rule.Error}");
                        RuleFor(x => x.Address)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEntityCommand.Address)} {rule.Error}");
                        RuleFor(x => x.Phone)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEntityCommand.Phone)} {rule.Error}");
                         RuleFor(x => x.Email)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEntityCommand.Email)} {rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.EntityName)
                            .MaximumLength(EntityNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateEntityCommand.EntityName)} {rule.Error} {EntityNameMaxLength}");
                        RuleFor(x => x.EntityDescription)
                            .MaximumLength(EntityDescriptionMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateEntityCommand.EntityDescription)} {rule.Error} {EntityDescriptionMaxLength}");
                        RuleFor(x => x.Address)
                            .MaximumLength(AddressMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateEntityCommand.Address)} {rule.Error} {AddressMaxLength}");
                        RuleFor(x => x.Phone)
                            .MaximumLength(PhoneMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateEntityCommand.Phone)} {rule.Error} {PhoneMaxLength}");
                              RuleFor(x => x.Email)
                            .MaximumLength(EmailMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateEntityCommand.Email)} {rule.Error} {EmailMaxLength}");
                        break;
                    case "MobileNumber": 
                        RuleFor(x => x.Phone) 
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                        .WithMessage($"{nameof(CreateEntityCommand.Phone)} {rule.Error}"); 
                        break; 
                    case "Email":
                        RuleFor(x => x.Email) 
                        .EmailAddress() 
                        .WithMessage($"{nameof(CreateEntityCommand.Email)} {rule.Error}"); 
                        break;
                    // case "AlphabeticOnly":
                    //     RuleFor(x => x.EntityName) 
                    //     .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                    //     .WithMessage($"{nameof(CreateEntityCommand.EntityName)} {rule.Error}");   
                    //     break;

                    case "NumericOnly":
                        RuleFor(x => x.Phone) 
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                        .WithMessage($"{nameof(CreateEntityCommand.Phone)} {rule.Error}");     
                        break;  
                    default:
                        // Handle unknown rule (log or throw)
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
         }
    }
}