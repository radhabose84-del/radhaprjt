using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.PriceGroupMaster
{
    public class UpdatePriceGroupMasterCommandValidator : AbstractValidator<UpdatePriceGroupMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPriceGroupMasterQueryRepository _queryRepository;

        public UpdatePriceGroupMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPriceGroupMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.PriceGroupMaster>("PriceGroupName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.PriceGroupMaster>("Description") ?? 255;

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
                        RuleFor(x => x.PriceGroupName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.EffectiveFrom)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PriceGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.PriceGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PriceGroupName)
                            .MustAsync(async (cmd, name, ct) => !await _queryRepository.NameAlreadyExistsAsync(name, cmd.Id))
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceGroupName));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Price Group ID is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Price Group {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.IsActive)} {rule.Error}");
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(UpdatePriceGroupMasterCommand.EffectiveTo)} {rule.Error} {nameof(UpdatePriceGroupMasterCommand.EffectiveFrom)}.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
