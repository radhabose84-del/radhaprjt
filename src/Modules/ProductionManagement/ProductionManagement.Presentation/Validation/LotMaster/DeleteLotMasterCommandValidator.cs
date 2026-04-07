using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.DeleteLotMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.LotMaster
{
    public class DeleteLotMasterCommandValidator : AbstractValidator<DeleteLotMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ILotMasterQueryRepository _queryRepository;

        public DeleteLotMasterCommandValidator(ILotMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteLotMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"LotMaster {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
