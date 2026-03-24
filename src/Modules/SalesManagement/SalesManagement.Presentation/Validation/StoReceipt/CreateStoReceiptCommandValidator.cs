using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoReceipt
{
    public class CreateStoReceiptCommandValidator : AbstractValidator<CreateStoReceiptCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoReceiptQueryRepository _queryRepository;

        public CreateStoReceiptCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IStoReceiptQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber = maxLengthProvider.GetMaxLength<Domain.Entities.StoReceiptHeader>("VehicleNumber") ?? 50;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.StoReceiptHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.DeliveryChallanHeaderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStoReceiptCommand.DeliveryChallanHeaderId)} {rule.Error}");

                        RuleFor(x => x.ReceivingPlantId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStoReceiptCommand.ReceivingPlantId)} {rule.Error}");

                        RuleFor(x => x.ReceivingStorageLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStoReceiptCommand.ReceivingStorageLocationId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(CreateStoReceiptCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateStoReceiptCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.DeliveryChallanHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.DeliveryChallanHeaderExistsAsync(id))
                            .WithMessage($"{nameof(CreateStoReceiptCommand.DeliveryChallanHeaderId)} {rule.Error}")
                            .When(x => x.DeliveryChallanHeaderId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.DeliveryChallanHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.IsDcApprovedAsync(id))
                            .WithMessage("Delivery Challan is not yet approved. Receipt can only be created for approved DCs.")
                            .When(x => x.DeliveryChallanHeaderId > 0);

                        RuleFor(x => x.DeliveryChallanHeaderId)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsStoReceiptExistsForDcAsync(id))
                            .WithMessage("A receipt already exists for this Delivery Challan.")
                            .When(x => x.DeliveryChallanHeaderId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.DeliveryChallanDetailId)
                                    .GreaterThan(0)
                                    .WithMessage($"DeliveryChallanDetailId {rule.Error}");

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

                                detail.RuleFor(d => d.ReceivedQuantity)
                                    .GreaterThan(0)
                                    .WithMessage($"ReceivedQuantity {rule.Error}");

                                detail.RuleFor(d => d.UOMId)
                                    .GreaterThan(0)
                                    .WithMessage($"UOMId {rule.Error}");

                                detail.RuleFor(d => d.NetWeight)
                                    .GreaterThan(0)
                                    .WithMessage($"NetWeight {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.DamageQuantity)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"DamageQuantity {rule.Error}");

                                detail.RuleFor(d => d.AcceptedQuantity)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"AcceptedQuantity {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    default:
                        break;
                }
            }

            // Business rules: quantity relationship validations
            RuleForEach(x => x.Details)
                .ChildRules(detail =>
                {
                    detail.RuleFor(d => d.ReceivedQuantity)
                        .LessThanOrEqualTo(d => d.DispatchQuantity)
                        .WithMessage("ReceivedQuantity must be less than or equal to DispatchQuantity.");

                    detail.RuleFor(d => d.DamageQuantity)
                        .LessThanOrEqualTo(d => d.ReceivedQuantity)
                        .WithMessage("DamageQuantity must be less than or equal to ReceivedQuantity.");

                    detail.RuleFor(d => d.AcceptedQuantity)
                        .Equal(d => d.ReceivedQuantity - d.DamageQuantity)
                        .WithMessage("AcceptedQuantity must be equal to ReceivedQuantity minus DamageQuantity.");
                })
                .When(x => x.Details != null && x.Details.Any());
        }
    }
}
