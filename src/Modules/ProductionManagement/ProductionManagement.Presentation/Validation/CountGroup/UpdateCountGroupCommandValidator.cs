using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CountGroup
{
    public class UpdateCountGroupCommandValidator : AbstractValidator<UpdateCountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICountGroupQueryRepository _queryRepo;

        public UpdateCountGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICountGroupQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                        RuleFor(x => x.CountGroupName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateCountGroupCommand.CountGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCountGroupCommand.CountGroupName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CountGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateCountGroupCommand.CountGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateCountGroupCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Count Group {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.CountGroupNameExistsAsync(cmd.CountGroupName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateCountGroupCommand.CountGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CountGroupName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateCountGroupCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
