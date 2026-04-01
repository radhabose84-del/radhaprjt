using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.DeleteRepackingMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingMaster
{
    public class DeleteRepackingMasterCommandValidator : AbstractValidator<DeleteRepackingMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingMasterQueryRepository _queryRepository;

        public DeleteRepackingMasterCommandValidator(IRepackingMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteRepackingMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Repacking Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
