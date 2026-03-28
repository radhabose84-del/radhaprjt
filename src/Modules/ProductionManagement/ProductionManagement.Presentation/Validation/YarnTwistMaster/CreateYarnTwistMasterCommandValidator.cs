using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnTwistMaster
{
    public class CreateYarnTwistMasterCommandValidator : AbstractValidator<CreateYarnTwistMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTwistMasterQueryRepository _queryRepo;

        public CreateYarnTwistMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IYarnTwistMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.YarnTwistMaster>("TwistName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.YarnTwistMaster>("Description") ?? 255;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.TwistName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.TwistName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.TwistName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.TwistName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.TwistName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TwistName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TwistName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.TwistName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TwistName)
                            .MustAsync(async (name, ct) => !await _queryRepo.TwistNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateYarnTwistMasterCommand.TwistName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TwistName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
