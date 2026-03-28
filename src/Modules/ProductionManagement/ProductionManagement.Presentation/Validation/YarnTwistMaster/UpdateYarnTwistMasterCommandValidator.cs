using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnTwistMaster
{
    public class UpdateYarnTwistMasterCommandValidator : AbstractValidator<UpdateYarnTwistMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTwistMasterQueryRepository _queryRepo;

        public UpdateYarnTwistMasterCommandValidator(
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
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.TwistName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.TwistName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TwistName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.TwistName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Yarn Twist Master {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.TwistNameExistsAsync(cmd.TwistName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.TwistName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TwistName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateYarnTwistMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
