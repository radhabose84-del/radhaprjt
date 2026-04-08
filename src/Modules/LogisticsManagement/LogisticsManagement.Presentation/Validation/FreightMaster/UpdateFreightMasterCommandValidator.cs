using FluentValidation;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using Shared.Validation.Common;

namespace LogisticsManagement.Presentation.Validation.FreightMaster
{
    public class UpdateFreightMasterCommandValidator : AbstractValidator<UpdateFreightMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightMasterQueryRepository _queryRepository;

        public UpdateFreightMasterCommandValidator(
            IFreightMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.FreightModeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.FreightModeId)} {rule.Error}");

                        RuleFor(x => x.RateMethodId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.RateMethodId)} {rule.Error}");

                        RuleFor(x => x.ModuleId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.ModuleId)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Rate)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.Rate)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"FreightMaster {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.FreightModeId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.FreightModeId)} {rule.Error}")
                            .When(x => x.FreightModeId > 0);

                        RuleFor(x => x.RateMethodId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.RateMethodId)} {rule.Error}")
                            .When(x => x.RateMethodId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                                !await _queryRepository.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, command.Id))
                            .WithMessage("The combination of FreightModeId, RateMethodId and ModuleId already exists.")
                            .When(x => x.FreightModeId > 0 && x.RateMethodId > 0 && x.ModuleId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateFreightMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Custom mode-method combination validation
            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                    await _queryRepository.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId))
                .WithMessage("Invalid FreightMode and RateMethod combination. PER_KM mode only allows PER_KM method. INNER/OUTER modes allow PER_KG, PER_BAG, or FIXED methods.")
                .When(x => x.FreightModeId > 0 && x.RateMethodId > 0);
        }
    }
}
