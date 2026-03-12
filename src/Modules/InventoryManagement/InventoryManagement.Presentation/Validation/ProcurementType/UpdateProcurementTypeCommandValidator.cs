using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.UpdateProcurementType;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.ProcurementType
{
    public class UpdateProcurementTypeCommandValidator : AbstractValidator<UpdateProcurementTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProcurementTypeQueryRepository _queryRepo;

        public UpdateProcurementTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProcurementTypeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                            .WithMessage($"{nameof(UpdateProcurementTypeCommand.ProcurementName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateProcurementTypeCommand.ProcurementName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProcurementName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateProcurementTypeCommand.ProcurementName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"ProcurementType {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateProcurementTypeCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
