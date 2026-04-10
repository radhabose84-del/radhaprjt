using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Presentation.Validation.ItemSpecificationValue
{
    public class UpdateItemSpecificationValueCommandValidator : AbstractValidator<UpdateItemSpecificationValueCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemSpecificationValueQueryRepository _queryRepo;

        public UpdateItemSpecificationValueCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IItemSpecificationValueQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthValue = maxLengthProvider.GetMaxLength<DomainEntities.ItemSpecificationValue>("SpecificationValue") ?? 100;

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
                        RuleFor(x => x.SpecificationValue)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationValue)
                            .MaximumLength(maxLengthValue)
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationValue)} {rule.Error} {maxLengthValue} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SpecificationValue)
                            .MustAsync(async (cmd, value, ct) =>
                                !await _queryRepo.AlreadyExistsAsync(cmd.SpecificationMasterId, value!, cmd.Id))
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationValue) && x.SpecificationMasterId > 0);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"ItemSpecificationValue {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SpecificationMasterId)
                            .MustAsync(async (id, ct) => await _queryRepo.SpecificationMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationMasterId)} {rule.Error}")
                            .When(x => x.SpecificationMasterId > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.SpecificationMasterId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.SpecificationMasterId)} {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateItemSpecificationValueCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Inactivate guard: block IsActive=0 when value is linked to ItemVariantValue or ItemItemSpecification
            RuleFor(x => x.Id)
                .MustAsync(async (cmd, id, ct) => !await _queryRepo.IsItemSpecificationValueLinkedAsync(id))
                .WithMessage("This master is linked with other records. You cannot inactivate this record.")
                .When(x => x.IsActive == 0);
        }
    }
}
