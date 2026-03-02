using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrder
{
    public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderQueryRepository _queryRepository;

        public CreateSalesOrderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOrderHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.SalesOrderDetails)
                            .NotNull()
                            .WithMessage($"SalesOrderDetails {rule.Error}");

                        RuleFor(x => x.SalesOrderDetails!.SalesGroupId)
                            .NotNull()
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.UnitId)
                            .NotNull()
                            .WithMessage($"UnitId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"UnitId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.PartyId)
                            .NotNull()
                            .WithMessage($"PartyId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.PaymentTermsId)
                            .NotNull()
                            .WithMessage($"PaymentTermsId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PaymentTermsId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.FreightTypeId)
                            .NotNull()
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.EnquiryType)
                            .NotNull()
                            .WithMessage($"EnquiryType {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"EnquiryType {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.DispatchLocationType)
                            .NotNull()
                            .WithMessage($"DispatchLocationType {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"DispatchLocationType {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);

                        RuleFor(x => x.SalesOrderDetails!.SalesOrderDetails)
                            .NotNull()
                            .WithMessage($"SalesOrderDetails {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesOrderDetails {rule.Error}")
                            .When(x => x.SalesOrderDetails != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesOrderDetails!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.SalesOrderDetails != null && !string.IsNullOrWhiteSpace(x.SalesOrderDetails.Remarks));
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.SalesOrderDetails!.SalesOrderDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.HSNId)
                                    .GreaterThan(0)
                                    .WithMessage($"HSNId {rule.Error}");

                                detail.RuleFor(d => d.QtyInBags)
                                    .GreaterThan(0)
                                    .WithMessage($"QtyInBags {rule.Error}");

                                detail.RuleFor(d => d.BagWeight)
                                    .GreaterThan(0)
                                    .WithMessage($"BagWeight {rule.Error}");

                                detail.RuleFor(d => d.SaleUOMId)
                                    .GreaterThan(0)
                                    .WithMessage($"SaleUOMId {rule.Error}");

                                detail.RuleFor(d => d.ExMillRate)
                                    .GreaterThan(0)
                                    .WithMessage($"ExMillRate {rule.Error}");
                            })
                            .When(x => x.SalesOrderDetails?.SalesOrderDetails != null && x.SalesOrderDetails.SalesOrderDetails.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.SalesOrderDetails!.SalesOrderDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.DiscountPerUnit)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"DiscountPerUnit {rule.Error}");

                                detail.RuleFor(d => d.Freight)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"Freight {rule.Error}");

                                detail.RuleFor(d => d.TaxPercentage)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"TaxPercentage {rule.Error}");

                                detail.RuleFor(d => d.TCSPercentage)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"TCSPercentage {rule.Error}");

                                detail.RuleFor(d => d.AgentCommissionPercentage)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"AgentCommissionPercentage {rule.Error}");
                            })
                            .When(x => x.SalesOrderDetails?.SalesOrderDetails != null && x.SalesOrderDetails.SalesOrderDetails.Any());
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOrderDetails!.SalesGroupId)
                            .MustAsync(async (salesGroupId, ct) =>
                                await _queryRepository.SalesGroupExistsAsync(salesGroupId))
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.SalesGroupId > 0);

                        RuleFor(x => x.SalesOrderDetails!.SalesSegmentId)
                            .MustAsync(async (salesSegmentId, ct) =>
                                await _queryRepository.SalesSegmentExistsAsync(salesSegmentId!.Value))
                            .WithMessage($"SalesSegmentId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.SalesSegmentId.HasValue && x.SalesOrderDetails.SalesSegmentId > 0);

                        RuleFor(x => x.SalesOrderDetails!.UnitId)
                            .MustAsync(async (unitId, ct) =>
                                await _queryRepository.UnitExistsAsync(unitId))
                            .WithMessage($"UnitId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.UnitId > 0);

                        RuleFor(x => x.SalesOrderDetails!.PartyId)
                            .MustAsync(async (partyId, ct) =>
                                await _queryRepository.PartyExistsAsync(partyId))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.PartyId > 0);

                        RuleFor(x => x.SalesOrderDetails!.PaymentTermsId)
                            .MustAsync(async (paymentTermsId, ct) =>
                                await _queryRepository.PaymentTermExistsAsync(paymentTermsId))
                            .WithMessage($"PaymentTermsId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.PaymentTermsId > 0);

                        RuleFor(x => x.SalesOrderDetails!.FreightTypeId)
                            .MustAsync(async (freightTypeId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(freightTypeId))
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.FreightTypeId > 0);

                        RuleFor(x => x.SalesOrderDetails!.DiscountPlanId)
                            .MustAsync(async (discountPlanId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(discountPlanId!.Value))
                            .WithMessage($"DiscountPlanId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.DiscountPlanId.HasValue && x.SalesOrderDetails.DiscountPlanId > 0);

                        RuleFor(x => x.SalesOrderDetails!.PaymentTypeId)
                            .MustAsync(async (paymentTypeId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(paymentTypeId!.Value))
                            .WithMessage($"PaymentTypeId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.PaymentTypeId.HasValue && x.SalesOrderDetails.PaymentTypeId > 0);

                        RuleFor(x => x.SalesOrderDetails!.CountListId)
                            .MustAsync(async (countListId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(countListId!.Value))
                            .WithMessage($"CountListId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.CountListId.HasValue && x.SalesOrderDetails.CountListId > 0);

                        RuleFor(x => x.SalesOrderDetails!.DispatchDepotId)
                            .MustAsync(async (depotId, ct) =>
                                await _queryRepository.WarehouseExistsAsync(depotId!.Value))
                            .WithMessage($"DispatchDepotId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.DispatchDepotId.HasValue && x.SalesOrderDetails.DispatchDepotId > 0);

                        RuleFor(x => x.SalesOrderDetails!.DispatchUnitId)
                            .MustAsync(async (dispatchUnitId, ct) =>
                                await _queryRepository.UnitExistsAsync(dispatchUnitId!.Value))
                            .WithMessage($"DispatchUnitId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.DispatchUnitId.HasValue && x.SalesOrderDetails.DispatchUnitId > 0);

                        // Detail-level FK validation
                        RuleForEach(x => x.SalesOrderDetails!.SalesOrderDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .MustAsync(async (itemId, ct) =>
                                        await _queryRepository.ItemExistsAsync(itemId))
                                    .WithMessage($"ItemId {rule.Error}")
                                    .When(d => d.ItemId > 0);

                                detail.RuleFor(d => d.HSNId)
                                    .MustAsync(async (hsnId, ct) =>
                                        await _queryRepository.HSNExistsAsync(hsnId))
                                    .WithMessage($"HSNId {rule.Error}")
                                    .When(d => d.HSNId > 0);

                                detail.RuleFor(d => d.SaleUOMId)
                                    .MustAsync(async (uomId, ct) =>
                                        await _queryRepository.UOMExistsAsync(uomId))
                                    .WithMessage($"SaleUOMId {rule.Error}")
                                    .When(d => d.SaleUOMId > 0);
                            })
                            .When(x => x.SalesOrderDetails?.SalesOrderDetails != null && x.SalesOrderDetails.SalesOrderDetails.Any());
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
