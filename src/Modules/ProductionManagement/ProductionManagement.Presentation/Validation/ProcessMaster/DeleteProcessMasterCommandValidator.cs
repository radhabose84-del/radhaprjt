using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.DeleteProcessMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.ProcessMaster
{
    public class DeleteProcessMasterCommandValidator : AbstractValidator<DeleteProcessMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProcessMasterQueryRepository _queryRepository;

        public DeleteProcessMasterCommandValidator(IProcessMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteProcessMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Process Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
