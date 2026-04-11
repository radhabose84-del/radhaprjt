using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RawMaterialType
{
    public class DeleteRawMaterialTypeCommandValidator : AbstractValidator<DeleteRawMaterialTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRawMaterialTypeQueryRepository _queryRepository;

        public DeleteRawMaterialTypeCommandValidator(IRawMaterialTypeQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteRawMaterialTypeCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Raw Material Type {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
