using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.VehicleMovementRecord
{
    public class DeleteVehicleMovementRecordCommandValidator : AbstractValidator<DeleteVehicleMovementRecordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;

        public DeleteVehicleMovementRecordCommandValidator(IVehicleMovementRecordQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteVehicleMovementRecordCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Vehicle Movement Record {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
