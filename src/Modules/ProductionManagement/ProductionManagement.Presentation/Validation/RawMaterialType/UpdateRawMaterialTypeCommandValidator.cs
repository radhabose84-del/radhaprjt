using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RawMaterialType
{
    public class UpdateRawMaterialTypeCommandValidator : AbstractValidator<UpdateRawMaterialTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRawMaterialTypeQueryRepository _queryRepository;

        public UpdateRawMaterialTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRawMaterialTypeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.RawMaterialType>("RawMaterialTypeName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.RawMaterialType>("Description") ?? 255;

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
                        RuleFor(x => x.RawMaterialTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.EffectiveFrom)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.RawMaterialTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.RawMaterialTypeName)
                            .MustAsync(async (cmd, name, ct) => !await _queryRepository.NameAlreadyExistsAsync(name, cmd.Id))
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RawMaterialTypeName));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Raw Material Type ID is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Raw Material Type {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.IsActive)} {rule.Error}");
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(UpdateRawMaterialTypeCommand.EffectiveTo)} {rule.Error} {nameof(UpdateRawMaterialTypeCommand.EffectiveFrom)}.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
