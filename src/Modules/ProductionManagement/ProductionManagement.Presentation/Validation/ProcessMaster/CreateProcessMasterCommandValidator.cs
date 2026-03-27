using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.CreateProcessMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.ProcessMaster
{
    public class CreateProcessMasterCommandValidator : AbstractValidator<CreateProcessMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProcessMasterQueryRepository _queryRepo;

        public CreateProcessMasterCommandValidator(
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
                            .WithMessage($"{nameof(CreateProcessMasterCommand.ProcessName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateProcessMasterCommand.ProcessName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.ProcessName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateProcessMasterCommand.ProcessName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProcessName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProcessName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateProcessMasterCommand.ProcessName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateProcessMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ProcessName)
                            .MustAsync(async (name, ct) => !await _queryRepo.ProcessNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateProcessMasterCommand.ProcessName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProcessName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
