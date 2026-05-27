using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.QualityParameter
{
    public class CreateQualityParameterCommandValidator : AbstractValidator<CreateQualityParameterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IUOMLookup _uomLookup;

        public CreateQualityParameterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IQualityParameterQueryRepository queryRepository,
            IUOMLookup uomLookup)
        {
            _queryRepository = queryRepository;
            _uomLookup = uomLookup;

            var maxLengthName = maxLengthProvider.GetMaxLength<QCManagement.Domain.Entities.QualityParameter>("ParameterName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<QCManagement.Domain.Entities.QualityParameter>("Description") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ParameterName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterName)} {rule.Error}");

                        RuleFor(x => x.ParameterGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterGroupId)} {rule.Error}");

                        RuleFor(x => x.DataTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateQualityParameterCommand.DataTypeId)} {rule.Error}");

                        RuleFor(x => x.ValidationTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ValidationTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ParameterName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateQualityParameterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrEmpty(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ParameterName)
                            .MustAsync(async (name, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(name!))
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ParameterName));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ParameterGroupId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ParameterGroupExistsAsync(id))
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ParameterGroupId)} {rule.Error}")
                            .When(x => x.ParameterGroupId > 0);

                        RuleFor(x => x.DataTypeId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.DataTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateQualityParameterCommand.DataTypeId)} {rule.Error}")
                            .When(x => x.DataTypeId > 0);

                        RuleFor(x => x.ValidationTypeId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ValidationTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateQualityParameterCommand.ValidationTypeId)} {rule.Error}")
                            .When(x => x.ValidationTypeId > 0);

                        // UnitId is conditional — only checked when DataType requires UOM.
                        // The conditional rule below handles existence + mandatory together.
                        break;

                    default:
                        break;
                }
            }

            // Conditional UOM rules — UnitId is mandatory when DataType is Numeric/Decimal,
            // and must be empty for other data types.
            RuleFor(x => x.UnitId)
                .MustAsync(async (cmd, unitId, ct) =>
                {
                    if (cmd.DataTypeId <= 0) return true; // FK validator handles this
                    var uomRequired = await _queryRepository.IsUomRequiredForDataTypeAsync(cmd.DataTypeId);
                    if (uomRequired && (!unitId.HasValue || unitId.Value <= 0))
                        return false; // Required but missing
                    if (!uomRequired && unitId.HasValue && unitId.Value > 0)
                        return false; // Provided but not applicable
                    return true;
                })
                .WithMessage("Unit of Measure is required for Numeric/Decimal data types, and must be empty otherwise.");

            RuleFor(x => x.UnitId)
                .MustAsync(async (unitId, ct) =>
                {
                    if (!unitId.HasValue || unitId.Value <= 0) return true;
                    var uoms = await _uomLookup.GetByIdsAsync(new[] { unitId.Value }, ct);
                    return uoms.Count > 0;
                })
                .WithMessage($"{nameof(CreateQualityParameterCommand.UnitId)} does not exist in UOM Master.")
                .When(x => x.UnitId.HasValue && x.UnitId.Value > 0);
        }
    }
}
