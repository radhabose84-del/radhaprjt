using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoTypeMaster
{
    public class CreateStoTypeMasterCommandValidator : AbstractValidator<CreateStoTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoTypeMasterQueryRepository _queryRepository;

        public CreateStoTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IStoTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.StoTypeMaster>("StoTypeCode") ?? 10;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.StoTypeMaster>("StoTypeName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.StoTypeMaster>("Description") ?? 250;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.StoTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeCode)} {rule.Error}");

                        RuleFor(x => x.StoTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeName)} {rule.Error}");

                        RuleFor(x => x.PgiMovementTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.PgiMovementTypeId)} {rule.Error}");

                        RuleFor(x => x.GrMovementTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.GrMovementTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.StoTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.StoTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PgiMovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.PgiMovementTypeId)} {rule.Error}")
                            .When(x => x.PgiMovementTypeId > 0);

                        RuleFor(x => x.GrMovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.GrMovementTypeId)} {rule.Error}")
                            .When(x => x.GrMovementTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.StoTypeCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateStoTypeMasterCommand.StoTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.StoTypeCode));
                        break;

                    default:
                        break;
                }
            }

            // Cross-field: PGI and GR Movement Types must be different
            RuleFor(x => x.GrMovementTypeId)
                .NotEqual(x => x.PgiMovementTypeId)
                .WithMessage("PGI Movement Type and GR Movement Type cannot be the same. Please select different values.")
                .When(x => x.PgiMovementTypeId > 0 && x.GrMovementTypeId > 0);
        }
    }
}
