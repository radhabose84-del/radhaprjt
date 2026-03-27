using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CertificationMaster
{
    public class UpdateCertificationMasterCommandValidator : AbstractValidator<UpdateCertificationMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICertificationMasterQueryRepository _queryRepo;

        public UpdateCertificationMasterCommandValidator(
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
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.CertificationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.CertificationName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CertificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.CertificationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Certification Master {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.CertificationNameExistsAsync(cmd.CertificationName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.CertificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CertificationName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateCertificationMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
