using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.MiscMaster
{
    public class CreateMiscMasterCommandValidator : AbstractValidator<CreateMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _queryRepo;

        public CreateMiscMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.MiscMaster>("Code") ?? 100;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.MiscMaster>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Code)
                            .NotNull().WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}");

                        RuleFor(x => x.Description)
                            .NotNull().WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error} {maxLengthCode} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Code));

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.MiscTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} must be greater than zero.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MiscTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} {rule.Error}")
                            .When(x => x.MiscTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.Code)
                            .MustAsync(async (cmd, code, ct) => !await _queryRepo.AlreadyExistsAsync(code!, cmd.MiscTypeId))
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Code));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
