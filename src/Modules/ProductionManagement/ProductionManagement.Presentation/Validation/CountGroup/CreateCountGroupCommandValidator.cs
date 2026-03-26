using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.CreateCountGroup;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CountGroup
{
    public class CreateCountGroupCommandValidator : AbstractValidator<CreateCountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICountGroupQueryRepository _queryRepo;

        public CreateCountGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICountGroupQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.CountGroup>("CountGroupCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.CountGroup>("CountGroupName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.CountGroup>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CountGroupCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupCode)} {rule.Error}");

                        RuleFor(x => x.CountGroupName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.CountGroupCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CountGroupCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CountGroupCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.CountGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateCountGroupCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CountGroupCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CountGroupCode));

                        RuleFor(x => x.CountGroupName)
                            .MustAsync(async (name, ct) => !await _queryRepo.CountGroupNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateCountGroupCommand.CountGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CountGroupName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
