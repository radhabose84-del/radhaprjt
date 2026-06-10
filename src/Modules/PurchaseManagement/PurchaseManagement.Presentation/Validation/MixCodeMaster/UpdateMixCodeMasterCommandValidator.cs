using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.MixCodeMaster
{
    public class UpdateMixCodeMasterCommandValidator : AbstractValidator<UpdateMixCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMixCodeMasterQueryRepository _queryRepo;

        public UpdateMixCodeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMixCodeMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                        RuleFor(x => x.MixCodeDesc)
                            .NotNull().WithMessage($"{nameof(UpdateMixCodeMasterCommand.MixCodeDesc)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateMixCodeMasterCommand.MixCodeDesc)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MixCodeDesc)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateMixCodeMasterCommand.MixCodeDesc)} {rule.Error} {maxLengthDesc} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"MixCodeMaster {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMixCodeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
