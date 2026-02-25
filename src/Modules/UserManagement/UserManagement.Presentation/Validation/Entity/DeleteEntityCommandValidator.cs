using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Entity
{
    public class DeleteEntityCommandValidator : AbstractValidator<DeleteEntityCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEntityQueryRepository _entityQueryRepository;
        public DeleteEntityCommandValidator( IEntityQueryRepository entityQueryRepository)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _entityQueryRepository = entityQueryRepository;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.EntityId)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteEntityCommand.EntityId)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.EntityId)
                      .MustAsync(async (Id, cancellation) => !await _entityQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}")
                        .When(x => x.EntityId > 0); // ✅ prevents calling repo for 0 / invalid id
                        break;
                    default:
                        
                        break;
                }
            }
        }
    }
}