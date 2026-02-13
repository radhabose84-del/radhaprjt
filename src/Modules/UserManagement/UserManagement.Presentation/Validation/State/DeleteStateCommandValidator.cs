using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Commands.DeleteState;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.State
{
    public class DeleteStateCommandValidator : AbstractValidator<DeleteStateCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStateQueryRepository _stateQueryRepository;

        // ✅ ADD optional rules parameter for unit testing
        public DeleteStateCommandValidator(
            IStateQueryRepository stateQueryRepository,
            IEnumerable<ValidationRule>? rules = null)
        {
            _stateQueryRepository = stateQueryRepository;

            _validationRules = (rules?.ToList() ?? ValidationRuleLoader.LoadValidationRules()) 
                               ?? new List<ValidationRule>();

            if (_validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteStateCommand.Id)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _stateQueryRepository.SoftDeleteValidation(id))
                            .WithMessage(rule.Error);
                        break;
                }
            }
        }
    }
}
