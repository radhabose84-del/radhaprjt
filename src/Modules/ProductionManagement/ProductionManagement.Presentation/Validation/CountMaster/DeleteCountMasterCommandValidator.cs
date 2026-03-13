using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CountMaster
{
    public class DeleteCountMasterCommandValidator : AbstractValidator<DeleteCountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICountMasterQueryRepository _queryRepository;

        public DeleteCountMasterCommandValidator(ICountMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCountMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Count Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
