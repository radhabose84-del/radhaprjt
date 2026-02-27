
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidator : AbstractValidator<CreateMiscTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;

        public CreateMiscTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.MiscTypeMaster>("MiscTypeCode") ?? 20;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.MiscTypeMaster>("Description") ?? 250;

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
                        RuleFor(x => x.MiscTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");

                        RuleFor(x => x.Description)
                            .NotNull()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.MiscTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MiscTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MiscTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MiscTypeCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code!))
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
