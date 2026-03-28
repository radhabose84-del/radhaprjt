using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnTwistMaster
{
    public class DeleteYarnTwistMasterCommandValidator : AbstractValidator<DeleteYarnTwistMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTwistMasterQueryRepository _queryRepository;

        public DeleteYarnTwistMasterCommandValidator(IYarnTwistMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteYarnTwistMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Yarn Twist Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
