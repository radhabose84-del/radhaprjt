using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CertificationMaster
{
    public class CreateCertificationMasterCommandValidator : AbstractValidator<CreateCertificationMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICertificationMasterQueryRepository _queryRepo;

        public CreateCertificationMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICertificationMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.CertificationMaster>("CertificationName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.CertificationMaster>("Description") ?? 255;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CertificationName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.CertificationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.CertificationName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.CertificationName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.CertificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CertificationName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CertificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.CertificationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CertificationName)
                            .MustAsync(async (name, ct) => !await _queryRepo.CertificationNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateCertificationMasterCommand.CertificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CertificationName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
