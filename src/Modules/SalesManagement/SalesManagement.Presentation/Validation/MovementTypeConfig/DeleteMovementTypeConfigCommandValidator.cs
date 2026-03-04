using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MovementTypeConfig
{
    public class DeleteMovementTypeConfigCommandValidator : AbstractValidator<DeleteMovementTypeConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMovementTypeConfigQueryRepository _queryRepository;

        public DeleteMovementTypeConfigCommandValidator(IMovementTypeConfigQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
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
                            .WithMessage($"{nameof(DeleteMovementTypeConfigCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Movement Type Config {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
