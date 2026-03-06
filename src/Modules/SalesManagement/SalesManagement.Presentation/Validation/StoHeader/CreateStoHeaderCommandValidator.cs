using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoHeader
{
    public class CreateStoHeaderCommandValidator : AbstractValidator<CreateStoHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoHeaderQueryRepository _queryRepository;

        public CreateStoHeaderCommandValidator(IStoHeaderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.StoTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.StoTypeId)} {rule.Error}");

                        RuleFor(x => x.MovementTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.MovementTypeId)} {rule.Error}");

                        RuleFor(x => x.SupplyingPlantId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.SupplyingPlantId)} {rule.Error}");

                        RuleFor(x => x.SupplyingStorageLocationId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.SupplyingStorageLocationId)} {rule.Error}");

                        RuleFor(x => x.ReceivingPlantId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.ReceivingPlantId)} {rule.Error}");

                        RuleFor(x => x.ReceivingStorageLocationId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.ReceivingStorageLocationId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StoTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.StoTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateStoHeaderCommand.StoTypeId)} {rule.Error}")
                            .When(x => x.StoTypeId > 0);

                        RuleFor(x => x.MovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateStoHeaderCommand.MovementTypeId)} {rule.Error}")
                            .When(x => x.MovementTypeId > 0);
                        break;

                    case "DateCompare":
                        RuleFor(x => x.ExpectedDeliveryDate)
                            .GreaterThanOrEqualTo(x => x.DocumentDate)
                            .WithMessage($"{nameof(CreateStoHeaderCommand.ExpectedDeliveryDate)} {rule.Error} {nameof(CreateStoHeaderCommand.DocumentDate)}.");
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.StoDetails).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.ItemId)
                                .GreaterThan(0)
                                .WithMessage($"ItemId {rule.Error}");

                            detail.RuleFor(d => d.Quantity)
                                .GreaterThan(0)
                                .WithMessage($"Quantity {rule.Error}");

                            detail.RuleFor(d => d.UOMId)
                                .GreaterThan(0)
                                .WithMessage($"UOMId {rule.Error}");
                        });
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.StoDetails).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.TransferPrice)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"TransferPrice {rule.Error}");
                        });
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
