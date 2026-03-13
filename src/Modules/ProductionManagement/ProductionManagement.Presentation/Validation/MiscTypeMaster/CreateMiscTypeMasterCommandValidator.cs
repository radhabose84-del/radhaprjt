using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidator : AbstractValidator<CreateMiscTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscTypeMasterQueryRepository _queryRepo;

        public CreateMiscTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscTypeMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.MiscTypeMaster>("MiscTypeCode") ?? 50;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.MiscTypeMaster>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.MiscTypeCode)
                            .NotNull().WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");

                        RuleFor(x => x.Description)
                            .NotNull().WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MiscTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error} {maxLengthDesc} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MiscTypeCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MiscTypeCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
