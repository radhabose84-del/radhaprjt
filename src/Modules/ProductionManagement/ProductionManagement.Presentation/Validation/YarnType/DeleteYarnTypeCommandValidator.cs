using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.DeleteYarnType;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnType
{
    public class DeleteYarnTypeCommandValidator : AbstractValidator<DeleteYarnTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTypeQueryRepository _queryRepository;

        public DeleteYarnTypeCommandValidator(IYarnTypeQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteYarnTypeCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Yarn Type {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
