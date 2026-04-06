using FluentValidation;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using LogisticsManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace LogisticsManagement.Presentation.Validation.MiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandValidator : AbstractValidator<UpdateMiscTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;

        public UpdateMiscTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthDescription = maxLengthProvider.GetMaxLength<LogisticsManagement.Domain.Entities.MiscTypeMaster>("Description") ?? 250;

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
                        RuleFor(x => x.Description)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.Description)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Misc Type Master {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
