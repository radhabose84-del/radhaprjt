using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.QualityMaster
{
    public class DeleteQualityMasterCommandValidator : AbstractValidator<DeleteQualityMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityMasterQueryRepository _queryRepository;

        public DeleteQualityMasterCommandValidator(IQualityMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteQualityMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Quality Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
