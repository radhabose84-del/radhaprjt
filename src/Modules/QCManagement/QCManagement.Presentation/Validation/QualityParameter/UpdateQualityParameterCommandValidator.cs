using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.QualityParameter
{
    public class UpdateQualityParameterCommandValidator : AbstractValidator<UpdateQualityParameterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IUOMLookup _uomLookup;

        public UpdateQualityParameterCommandValidator(
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
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterName)} {rule.Error}");

                        RuleFor(x => x.ParameterGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterGroupId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ParameterName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrEmpty(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Quality Parameter {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ParameterGroupId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.ParameterGroupExistsAsync(id))
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterGroupId)} {rule.Error}")
                            .When(x => x.ParameterGroupId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ParameterName)
                            .MustAsync(async (cmd, name, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(name!, cmd.Id))
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.ParameterName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ParameterName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateQualityParameterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Conditional UOM — fetch the existing DataTypeId to determine if UOM is required.
            // DataType is immutable on Update, so this honours the integrity rule from Create.
            RuleFor(x => x.UnitId)
                .MustAsync(async (cmd, unitId, ct) =>
                {
                    var dataTypeId = await _queryRepository.GetDataTypeIdByQualityParameterIdAsync(cmd.Id);
                    if (!dataTypeId.HasValue) return true; // Id-not-found handled elsewhere
                    var uomRequired = await _queryRepository.IsUomRequiredForDataTypeAsync(dataTypeId.Value);
                    if (uomRequired && (!unitId.HasValue || unitId.Value <= 0))
                        return false;
                    if (!uomRequired && unitId.HasValue && unitId.Value > 0)
                        return false;
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
                .WithMessage($"{nameof(UpdateQualityParameterCommand.UnitId)} does not exist in UOM Master.")
                .When(x => x.UnitId.HasValue && x.UnitId.Value > 0);
        }
    }
}
