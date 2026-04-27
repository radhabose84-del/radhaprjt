using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DeliveryChallan
{
    public class CreateDeliveryChallanCommandValidator : AbstractValidator<CreateDeliveryChallanCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryChallanQueryRepository _queryRepository;

        public CreateDeliveryChallanCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDeliveryChallanQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber = maxLengthProvider.GetMaxLength<Domain.Entities.DeliveryChallanHeader>("VehicleNumber") ?? 50;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.DeliveryChallanHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.StoHeaderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.StoHeaderId)} {rule.Error}");

                        RuleFor(x => x.FromPlantId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.FromPlantId)} {rule.Error}");

                        RuleFor(x => x.ToPlantId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.ToPlantId)} {rule.Error}");

                        RuleFor(x => x.FromStorageLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.FromStorageLocationId)} {rule.Error}");

                        RuleFor(x => x.ToStorageLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.ToStorageLocationId)} {rule.Error}");

                        RuleFor(x => x.TransporterId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.TransporterId)} {rule.Error}");

                        RuleFor(x => x.DcTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.DcTypeId)} {rule.Error}");

                        RuleFor(x => x.MovementTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.MovementTypeId)} {rule.Error}");

                        RuleFor(x => x.VehicleNumber)
                            .NotNull()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.VehicleNumber)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.VehicleNumber)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StoHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.StoHeaderExistsAsync(id))
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.StoHeaderId)} {rule.Error}")
                            .When(x => x.StoHeaderId > 0);

                        RuleFor(x => x.DcTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.DcTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.DcTypeId)} {rule.Error}")
                            .When(x => x.DcTypeId > 0);

                        RuleFor(x => x.MovementTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MovementTypeConfigExistsAsync(id))
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.MovementTypeId)} {rule.Error}")
                            .When(x => x.MovementTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.StoHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.IsStoApprovedAsync(id))
                            .WithMessage("STO is not yet approved. Delivery Challan can only be created for approved STOs.")
                            .When(x => x.StoHeaderId > 0);

                        RuleFor(x => x.StoHeaderId)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsStoFullyDispatchedAsync(id))
                            .WithMessage("All items in this STO are fully dispatched. No further Delivery Challan can be created.")
                            .When(x => x.StoHeaderId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.StoDetailId)
                                    .GreaterThan(0)
                                    .WithMessage($"StoDetailId {rule.Error}");

                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.LotId)
                                    .GreaterThan(0)
                                    .WithMessage($"LotId {rule.Error}");

                                detail.RuleFor(d => d.StartPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"StartPackNo {rule.Error}");

                                detail.RuleFor(d => d.EndPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"EndPackNo {rule.Error}");

                                detail.RuleFor(d => d.DispatchQuantity)
                                    .GreaterThan(0)
                                    .WithMessage($"DispatchQuantity {rule.Error}");

                                detail.RuleFor(d => d.DispatchQuantity)
                                    .MustAsync(async (dto, qty, ct) =>
                                    {
                                        var openQty = await _queryRepository.GetStoOpenQtyAsync(dto.StoDetailId);
                                        return openQty != null && qty <= openQty.OpenQty;
                                    })
                                    .WithMessage("DispatchQuantity exceeds available STO open quantity.")
                                    .When(d => d.StoDetailId > 0 && d.DispatchQuantity > 0);

                                detail.RuleFor(d => d.UOMId)
                                    .GreaterThan(0)
                                    .WithMessage($"UOMId {rule.Error}");

                                detail.RuleFor(d => d.NetWeight)
                                    .GreaterThan(0)
                                    .WithMessage($"NetWeight {rule.Error}");

                                detail.RuleFor(d => d.ExMillRate)
                                    .GreaterThan(0)
                                    .WithMessage($"ExMillRate {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.ConsignmentValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDeliveryChallanCommand.ConsignmentValue)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
