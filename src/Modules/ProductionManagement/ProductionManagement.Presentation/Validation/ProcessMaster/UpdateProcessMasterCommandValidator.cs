using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.UpdateProcessMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.ProcessMaster
{
    public class UpdateProcessMasterCommandValidator : AbstractValidator<UpdateProcessMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProcessMasterQueryRepository _queryRepo;

        public UpdateProcessMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProcessMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.ProcessMaster>("ProcessName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.ProcessMaster>("Description") ?? 255;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ProcessName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.ProcessName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.ProcessName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProcessName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.ProcessName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Process Master {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.ProcessNameExistsAsync(cmd.ProcessName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.ProcessName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProcessName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateProcessMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
