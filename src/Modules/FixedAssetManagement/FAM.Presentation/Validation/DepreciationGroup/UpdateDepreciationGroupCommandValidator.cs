using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Domain.Entities;
using FAM.Presentation.Validation.Common;
using FAM.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using static FAM.Domain.Common.BaseEntity;
using Shared.Validation.Common;


namespace FAM.Presentation.Validation.DepreciationGroup
{
    public class UpdateDepreciationGroupCommandValidator  : AbstractValidator<UpdateDepreciationGroupCommand>
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly ApplicationDbContext _applicationDbContext; 
        public UpdateDepreciationGroupCommandValidator(MaxLengthProvider maxLengthProvider,ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            // Get max lengths dynamically using MaxLengthProvider
            var depreciationGroupCodeMaxLength = maxLengthProvider.GetMaxLength<DepreciationGroups>("Code")??10;
            var depreciationGroupNameMaxLength = maxLengthProvider.GetMaxLength<DepreciationGroups>("DepreciationGroupName")??50;            

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
                        RuleFor(x => x.DepreciationGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.DepreciationGroupName)} {rule.Error}");
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.Code)} {rule.Error}");
                        RuleFor(x => x.UsefulLife)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.UsefulLife)} {rule.Error}");
                        RuleFor(x => x.ResidualValue)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.ResidualValue)} {rule.Error}");
                        RuleFor(x => x.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.AssetGroupId)} {rule.Error}");
                        RuleFor(x => x.DepreciationMethod)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.DepreciationMethod)} {rule.Error}");
                        RuleFor(x => x.BookType)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.BookType)} {rule.Error}");                        
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.DepreciationGroupName)
                            .MaximumLength(depreciationGroupNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.DepreciationGroupName)} {rule.Error} {depreciationGroupNameMaxLength}");
                        RuleFor(x => x.Code)
                            .MaximumLength(depreciationGroupCodeMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.Code)} {rule.Error} {depreciationGroupCodeMaxLength}");                    
                        RuleFor(x => x.ResidualValue)
                            .InclusiveBetween(1,100)
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.ResidualValue)} {rule.Error} 100");   
                        RuleFor(x => x.UsefulLife)
                            .InclusiveBetween(1, 100)
                            .WithMessage($"{nameof(UpdateDepreciationGroupCommand.UsefulLife)} {rule.Error} 100");   
                        break;
                     case "UniqueCombination":
                        RuleFor(x => x)
                        .MustAsync(async (command, token) =>
                        {
                            return !await _applicationDbContext.DepreciationGroups.AnyAsync(x =>
                                x.AssetGroupId == command.AssetGroupId &&
                                x.DepreciationMethod == command.DepreciationMethod &&
                                x.IsDeleted==IsDelete.NotDeleted &&
                                x.IsActive == Status.Active && x.Id != command.Id, token);
                        })
                        .WithMessage(rule.Error);
                        break;
                    default:                        
                        break;
                }
            }
            // Allow only alphanumeric + space (A–Z, a–z, 0–9, space)
            const string alphaNumWithSpacePattern = @"^[A-Za-z0-9 ]+$";

            RuleFor(x => (x.Code ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.Code))
                .WithMessage($"{nameof(UpdateDepreciationGroupCommand.Code)} must be alphanumeric only. Special characters are not allowed.");

            RuleFor(x => (x.DepreciationGroupName ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.DepreciationGroupName))
                .WithMessage($"{nameof(UpdateDepreciationGroupCommand.DepreciationGroupName)} must be alphanumeric only. Special characters are not allowed.");
        }
    }
}