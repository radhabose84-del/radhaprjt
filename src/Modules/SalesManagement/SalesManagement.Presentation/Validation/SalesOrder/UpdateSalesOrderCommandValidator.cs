using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrder
{
    public class UpdateSalesOrderCommandValidator : AbstractValidator<UpdateSalesOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateSalesOrderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

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
                        RuleFor(x => x.SalesGroupId)
                            .NotNull()
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesGroupId {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotNull()
                            .WithMessage($"UnitId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"UnitId {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotNull()
                            .WithMessage($"PartyId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PartyId {rule.Error}");

                        RuleFor(x => x.FreightTypeId)
                            .NotNull()
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"FreightTypeId {rule.Error}");

                        RuleFor(x => x.EnquiryType)
                            .NotNull()
                            .WithMessage($"EnquiryType {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"EnquiryType {rule.Error}");

                        RuleFor(x => x.SalesOrderDetails)
                            .NotNull()
                            .WithMessage($"SalesOrderDetails {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesOrderDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesOrder {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"IsActive {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.SalesOrderDetails)
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
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.SalesOrderDetails)
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
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.Any());
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesGroupId)
                            .MustAsync(async (salesGroupId, ct) =>
                                await _queryRepository.SalesGroupExistsAsync(salesGroupId))
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .When(x => x.SalesGroupId > 0);

                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (salesSegmentId, ct) =>
                                await _queryRepository.SalesSegmentExistsAsync(salesSegmentId!.Value))
                            .WithMessage($"SalesSegmentId {rule.Error}")
                            .When(x => x.SalesSegmentId.HasValue && x.SalesSegmentId > 0);

                        RuleFor(x => x.UnitId)
                            .MustAsync(async (unitId, ct) =>
                                await _queryRepository.UnitExistsAsync(unitId!.Value))
                            .WithMessage($"UnitId {rule.Error}")
                            .When(x => x.UnitId.HasValue && x.UnitId > 0);

                        RuleFor(x => x.PartyId)
                            .MustAsync(async (partyId, ct) =>
                                await _queryRepository.PartyExistsAsync(partyId))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.PartyId > 0);

                        RuleFor(x => x.EnquiryType)
                            .MustAsync(async (enquiryType, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(enquiryType))
                            .WithMessage($"EnquiryType {rule.Error}")
                            .When(x => x.EnquiryType > 0);

                        RuleFor(x => x.FreightTypeId)
                            .MustAsync(async (freightTypeId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(freightTypeId))
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .When(x => x.FreightTypeId > 0);

                        RuleFor(x => x.PaymentTypeId)
                            .MustAsync(async (paymentTypeId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(paymentTypeId!.Value))
                            .WithMessage($"PaymentTypeId {rule.Error}")
                            .When(x => x.PaymentTypeId.HasValue && x.PaymentTypeId > 0);

                        RuleFor(x => x.CountListId)
                            .MustAsync(async (countListId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(countListId!.Value))
                            .WithMessage($"CountListId {rule.Error}")
                            .When(x => x.CountListId.HasValue && x.CountListId > 0);

                        RuleFor(x => x.SubAgentId)
                            .MustAsync(async (subAgentId, ct) =>
                                await _queryRepository.SubAgentExistsAsync(subAgentId!.Value))
                            .WithMessage($"SubAgentId {rule.Error}")
                            .When(x => x.SubAgentId.HasValue && x.SubAgentId > 0);

                        // Detail-level FK validation
                        RuleForEach(x => x.SalesOrderDetails)
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

                                detail.RuleFor(d => d.LineItemStatusId)
                                    .MustAsync(async (statusId, ct) =>
                                        await _queryRepository.MiscMasterExistsAsync(statusId!.Value))
                                    .WithMessage($"LineItemStatusId {rule.Error}")
                                    .When(d => d.LineItemStatusId.HasValue && d.LineItemStatusId > 0);
                            })
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.Any());
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id, ct))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.PartyId > 0);

                        RuleFor(x => x.SubAgentId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessAgentAsync(id!.Value, ct))
                            .WithMessage($"SubAgentId {rule.Error}")
                            .When(x => x.SubAgentId.HasValue && x.SubAgentId > 0);
                        break;

                    default:
                        break;
                }
            }

            // MD Discount — when checkbox enabled, Rate + Document are mandatory
            // MD Discount — when checkbox enabled:
            //   • Either MdDiscountRate OR MdDiscountPercentage must be provided (at least one)
            //   • Whichever is provided must be > 0
            RuleFor(x => x)
                .Must(cmd => (cmd.MdDiscountRate.HasValue && cmd.MdDiscountRate.Value > 0)
                          || (cmd.MdDiscountPercentage.HasValue && cmd.MdDiscountPercentage.Value > 0))
                .WithMessage("Either MdDiscountRate or MdDiscountPercentage is required (and must be greater than zero) when MD Discount is enabled.")
                .When(x => x.IsMdDiscountEnabled);

            RuleFor(x => x.MdApprovalDocument)
                .NotEmpty().WithMessage("MdApprovalDocument is required when MD Discount is enabled.")
                .When(x => x.IsMdDiscountEnabled);

            // Applied discounts — optional; when present: max 3, unique SlabTypeId, unique DiscountMasterId, FKs valid
            RuleFor(x => x.Discounts)
                .Must(d => d == null || d.Count <= 3)
                .WithMessage("A Sales Order can have at most 3 discounts.");

            RuleFor(x => x.Discounts)
                .Must(d => d == null || d.Select(x => x.SlabTypeId).Distinct().Count() == d.Count)
                .WithMessage("Each applied discount must have a unique SlabTypeId.")
                .When(x => x.Discounts != null && x.Discounts.Any());

            RuleFor(x => x.Discounts)
                .Must(d => d == null || d.Select(x => x.DiscountMasterId).Distinct().Count() == d.Count)
                .WithMessage("Each applied discount must reference a unique DiscountMaster.")
                .When(x => x.Discounts != null && x.Discounts.Any());

            RuleForEach(x => x.Discounts)
                .ChildRules(d =>
                {
                    d.RuleFor(x => x.DiscountMasterId)
                        .GreaterThan(0).WithMessage("DiscountMasterId must be greater than zero.")
                        .MustAsync(async (id, ct) => await _queryRepository.DiscountMasterExistsAsync(id))
                        .WithMessage("DiscountMaster does not exist or is inactive/deleted.")
                        .When(x => x.DiscountMasterId > 0);

                    d.RuleFor(x => x.SlabTypeId)
                        .GreaterThan(0).WithMessage("SlabTypeId must be greater than zero.");

                    d.RuleFor(x => x.PaymentTermId)
                        .GreaterThan(0).WithMessage("PaymentTermId must be greater than zero.");
                })
                .When(x => x.Discounts != null && x.Discounts.Any());
        }
    }
}
