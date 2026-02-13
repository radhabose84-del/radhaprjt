using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Commands.DeleteModule;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.Module
{
    public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IModuleQueryRepository _moduleQueryRepository;
        public DeleteModuleCommandValidator(IModuleQueryRepository moduleQueryRepository)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _moduleQueryRepository = moduleQueryRepository;

              if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

                foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ModuleId)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteModuleCommand.ModuleId)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.ModuleId)
                      .MustAsync(async (Id, cancellation) => !await _moduleQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        
                        break;
                }
            }

        }
        
    }
}