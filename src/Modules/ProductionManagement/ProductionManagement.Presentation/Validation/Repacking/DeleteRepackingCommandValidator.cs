using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.DeleteRepacking;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.Repacking
{
    public class DeleteRepackingCommandValidator : AbstractValidator<DeleteRepackingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingQueryRepository _queryRepository;

        public DeleteRepackingCommandValidator(IRepackingQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteRepackingCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Repacking {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
