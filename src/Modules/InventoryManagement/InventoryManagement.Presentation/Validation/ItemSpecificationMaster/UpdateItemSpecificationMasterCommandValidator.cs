using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Presentation.Validation.ItemSpecificationMaster
{
    public class UpdateItemSpecificationMasterCommandValidator : AbstractValidator<UpdateItemSpecificationMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemSpecificationMasterQueryRepository _queryRepo;

        public UpdateItemSpecificationMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IItemSpecificationMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<DomainEntities.ItemSpecificationMaster>("SpecificationName") ?? 100;

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
                        RuleFor(x => x.SpecificationName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.SpecificationName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SpecificationName)
                            .MustAsync(async (cmd, name, ct) => !await _queryRepo.NameAlreadyExistsAsync(name!, cmd.Id))
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationName));

                        RuleFor(x => x.Order)
                            .MustAsync(async (cmd, order, ct) => !await _queryRepo.OrderAlreadyExistsAsync(order, cmd.Id))
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.Order)} {rule.Error}")
                            .When(x => x.Order > 0);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"ItemSpecificationMaster {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Order)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.Order)} {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateItemSpecificationMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Inactivate guard: block IsActive=0 when master is linked to ItemVariantAttribute
            RuleFor(x => x.Id)
                .MustAsync(async (cmd, id, ct) => !await _queryRepo.IsItemSpecificationMasterLinkedAsync(id))
                .WithMessage("This master is linked with other records. You cannot inactivate this record.")
                .When(x => x.IsActive == 0);
        }
    }
}
