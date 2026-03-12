using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.DeleteProcurementType;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.ProcurementType
{
    public class DeleteProcurementTypeCommandValidator : AbstractValidator<DeleteProcurementTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProcurementTypeQueryRepository _queryRepository;

        public DeleteProcurementTypeCommandValidator(IProcurementTypeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteProcurementTypeCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ProcurementType {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
