using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.QualityMaster
{
    public class CreateQualityMasterCommandValidator : AbstractValidator<CreateQualityMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityMasterQueryRepository _queryRepo;

        public CreateQualityMasterCommandValidator(
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
                            .WithMessage($"{nameof(CreateQualityMasterCommand.QualityName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateQualityMasterCommand.QualityName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.QualityName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateQualityMasterCommand.QualityName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.QualityName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.QualityName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateQualityMasterCommand.QualityName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateQualityMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.QualityName)
                            .MustAsync(async (name, ct) => !await _queryRepo.QualityNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateQualityMasterCommand.QualityName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.QualityName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
