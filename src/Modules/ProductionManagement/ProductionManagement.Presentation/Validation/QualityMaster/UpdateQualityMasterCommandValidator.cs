using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.QualityMaster
{
    public class UpdateQualityMasterCommandValidator : AbstractValidator<UpdateQualityMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityMasterQueryRepository _queryRepo;

        public UpdateQualityMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IQualityMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.QualityMaster>("QualityName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.QualityMaster>("Description") ?? 255;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.QualityName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.QualityName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.QualityName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.QualityName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.QualityName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Quality Master {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.QualityNameExistsAsync(cmd.QualityName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.QualityName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.QualityName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateQualityMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
