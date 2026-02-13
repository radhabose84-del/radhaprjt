using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Domain.Entities;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Languages
{
    public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateLanguageCommandValidator(MaxLengthProvider maxLengthProvider)
        {
             var nameMaxLength = maxLengthProvider.GetMaxLength<Language>("Name") ?? 50;
            var codeMaxLength = maxLengthProvider.GetMaxLength<Language>("Code") ?? 10;

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
                        RuleFor(x => x.Name)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateLanguageCommand.Name)} {rule.Error}");
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateLanguageCommand.Code)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.Name)
                            .MaximumLength(nameMaxLength)
                            .WithMessage($"{nameof(UpdateLanguageCommand.Name)} {rule.Error} {nameMaxLength}");
                        RuleFor(x => x.Code)
                            .MaximumLength(codeMaxLength)
                            .WithMessage($"{nameof(UpdateLanguageCommand.Code)} {rule.Error} {codeMaxLength}");
                        break;
                }
            }
        }
    }
}