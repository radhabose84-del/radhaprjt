using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.CreateProcurementType;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.ProcurementType
{
    public class CreateProcurementTypeCommandValidator : AbstractValidator<CreateProcurementTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateProcurementTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProcurementTypeQueryRepository queryRepo)
        {
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.ProcurementType>("ProcurementName") ?? 100;

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
                        RuleFor(x => x.ProcurementName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateProcurementTypeCommand.ProcurementName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateProcurementTypeCommand.ProcurementName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProcurementName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateProcurementTypeCommand.ProcurementName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
