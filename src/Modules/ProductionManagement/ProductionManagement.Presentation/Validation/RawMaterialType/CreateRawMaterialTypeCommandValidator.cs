using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RawMaterialType
{
    public class CreateRawMaterialTypeCommandValidator : AbstractValidator<CreateRawMaterialTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRawMaterialTypeQueryRepository _queryRepository;

        public CreateRawMaterialTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRawMaterialTypeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.RawMaterialType>("RawMaterialTypeCode") ?? 20;
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
                        RuleFor(x => x.RawMaterialTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeCode)} {rule.Error}");

                        RuleFor(x => x.RawMaterialTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.EffectiveFrom)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.RawMaterialTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RawMaterialTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.RawMaterialTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.RawMaterialTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.RawMaterialTypeCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RawMaterialTypeCode));

                        RuleFor(x => x.RawMaterialTypeName)
                            .MustAsync(async (name, ct) => !await _queryRepository.NameAlreadyExistsAsync(name))
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.RawMaterialTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RawMaterialTypeName));
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(CreateRawMaterialTypeCommand.EffectiveTo)} {rule.Error} {nameof(CreateRawMaterialTypeCommand.EffectiveFrom)}.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
