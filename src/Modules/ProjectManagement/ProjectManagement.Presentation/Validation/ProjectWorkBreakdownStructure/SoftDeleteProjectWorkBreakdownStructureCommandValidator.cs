using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using FluentValidation;
using Shared.Validation.Common;

namespace ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure
{
    public class DeleteProjectWorkBreakdownStructureCommandValidator : AbstractValidator<DeleteProjectWorkBreakdownStructureCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepository;

        public DeleteProjectWorkBreakdownStructureCommandValidator(IProjectWorkBreakdownStructureQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteProjectWorkBreakdownStructureCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ProjectWorkBreakdownStructure {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
