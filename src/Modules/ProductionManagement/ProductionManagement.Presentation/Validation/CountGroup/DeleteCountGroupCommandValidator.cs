using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CountGroup
{
    public class DeleteCountGroupCommandValidator : AbstractValidator<DeleteCountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICountGroupQueryRepository _queryRepository;

        public DeleteCountGroupCommandValidator(ICountGroupQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCountGroupCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Count Group {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
