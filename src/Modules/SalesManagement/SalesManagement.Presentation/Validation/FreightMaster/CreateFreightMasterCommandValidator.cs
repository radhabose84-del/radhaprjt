using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.FreightMaster
{
    public class CreateFreightMasterCommandValidator : AbstractValidator<CreateFreightMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightMasterQueryRepository _queryRepo;

        public CreateFreightMasterCommandValidator(IFreightMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateFreightMasterCommand.FreightModeId)} {rule.Error}");

                        RuleFor(x => x.RateMethodId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateFreightMasterCommand.RateMethodId)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Rate)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateFreightMasterCommand.Rate)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.FreightModeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateFreightMasterCommand.FreightModeId)} {rule.Error}")
                            .When(x => x.FreightModeId > 0);

                        RuleFor(x => x.RateMethodId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateFreightMasterCommand.RateMethodId)} {rule.Error}")
                            .When(x => x.RateMethodId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.CompositeKeyExistsAsync(cmd.FreightModeId, cmd.RateMethodId))
                            .WithMessage("This Freight Mode and Rate Method combination already exists.")
                            .When(x => x.FreightModeId > 0 && x.RateMethodId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Custom: Mode-Method combination validation
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) => await _queryRepo.IsValidModeMethodCombinationAsync(cmd.FreightModeId, cmd.RateMethodId))
                .WithMessage("Invalid Freight Mode and Rate Method combination. PER_KM mode allows only PER_KM method. INNER/OUTER modes allow PER_KG, PER_BAG, or FIXED methods.")
                .When(x => x.FreightModeId > 0 && x.RateMethodId > 0);
        }
    }
}
