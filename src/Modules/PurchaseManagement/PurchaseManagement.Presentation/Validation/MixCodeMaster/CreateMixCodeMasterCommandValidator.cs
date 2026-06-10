using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.MixCodeMaster
{
    public class CreateMixCodeMasterCommandValidator : AbstractValidator<CreateMixCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMixCodeMasterQueryRepository _queryRepo;

        public CreateMixCodeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMixCodeMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.MixCodeMaster>("MixCode") ?? 20;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.MixCodeMaster>("MixCodeDesc") ?? 100;

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
                        RuleFor(x => x.MixCode)
                            .NotNull().WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCode)} {rule.Error}");

                        RuleFor(x => x.MixCodeDesc)
                            .NotNull().WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCodeDesc)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCodeDesc)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.MixCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MixCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MixCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.MixCodeDesc)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCodeDesc)} {rule.Error} {maxLengthDesc} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MixCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code, null))
                            .WithMessage($"{nameof(CreateMixCodeMasterCommand.MixCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MixCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
