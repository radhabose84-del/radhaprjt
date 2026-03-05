using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoTypeMaster
{
    public class UpdateStoTypeMasterCommandValidator : AbstractValidator<UpdateStoTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoTypeMasterQueryRepository _queryRepository;

        public UpdateStoTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IStoTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.StoTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.StoTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.StoTypeName)} {rule.Error}");

                        RuleFor(x => x.PgiMovementTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.PgiMovementTypeId)} {rule.Error}");

                        RuleFor(x => x.GrMovementTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.GrMovementTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.StoTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.StoTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"STO Type Master {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PgiMovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.PgiMovementTypeId)} {rule.Error}")
                            .When(x => x.PgiMovementTypeId > 0);

                        RuleFor(x => x.GrMovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.GrMovementTypeId)} {rule.Error}")
                            .When(x => x.GrMovementTypeId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateStoTypeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
