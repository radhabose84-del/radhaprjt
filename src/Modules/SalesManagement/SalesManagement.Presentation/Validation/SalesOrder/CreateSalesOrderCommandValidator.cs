using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrder
{
    public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CreateSalesOrderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderQueryRepository queryRepository,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;
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

                        RuleFor(x => x.SalesOrderDetails!.SalesOrderTypeId)
                            .NotNull()
                            .WithMessage($"SalesOrderTypeId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesOrderTypeId {rule.Error}")
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

                        RuleFor(x => x.SalesOrderDetails!.EnquiryType)
                            .MustAsync(async (enquiryType, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(enquiryType))
                            .WithMessage($"EnquiryType {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.EnquiryType > 0);

                        RuleFor(x => x.SalesOrderDetails!.FreightTypeId)
                            .MustAsync(async (freightTypeId, ct) =>
                                await _queryRepository.MiscMasterExistsAsync(freightTypeId))
                            .WithMessage($"FreightTypeId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.FreightTypeId > 0);

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

                        RuleFor(x => x.SalesOrderDetails!.SubAgentId)
                            .MustAsync(async (subAgentId, ct) =>
                                await _queryRepository.SubAgentExistsAsync(subAgentId!.Value))
                            .WithMessage($"SubAgentId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.SubAgentId.HasValue && x.SalesOrderDetails.SubAgentId > 0);

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

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.SalesOrderDetails!.PartyId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id, ct))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.PartyId > 0);

                        RuleFor(x => x.SalesOrderDetails!.SubAgentId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessAgentAsync(id!.Value, ct))
                            .WithMessage($"SubAgentId {rule.Error}")
                            .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.SubAgentId.HasValue && x.SalesOrderDetails.SubAgentId > 0);
                        break;

                    case "Workflow":
                        RuleFor(x => x.SalesOrderDetails)
                            .MustAsync(async (details, cancellation) =>
                            {
                                var orderUnitId = _ipAddressService.GetUnitId() ?? 0;
                                if (orderUnitId <= 0) return false;
                                return await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                    MiscEnumEntity.TransactionTypeSalesOrder,
                                    orderUnitId,
                                    0);
                            })
                            .WithMessage(rule.Error)
                            .When(x => x.SalesOrderDetails != null);
                        break;

                    default:
                        break;
                }
            }

            // MD Discount — when checkbox enabled, Rate + Document are mandatory
            RuleFor(x => x.SalesOrderDetails!.MdDiscountRate)
                .NotNull().WithMessage("MdDiscountRate is required when MD Discount is enabled.")
                .GreaterThan(0).WithMessage("MdDiscountRate must be greater than zero.")
                .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.IsMdDiscountEnabled);

            RuleFor(x => x.SalesOrderDetails!.MdApprovalDocument)
                .NotEmpty().WithMessage("MdApprovalDocument is required when MD Discount is enabled.")
                .When(x => x.SalesOrderDetails != null && x.SalesOrderDetails.IsMdDiscountEnabled);

            // Applied discounts — optional; when present: max 3, unique SlabTypeId, unique DiscountMasterId, FKs valid
            RuleFor(x => x.SalesOrderDetails!.Discounts)
                .Must(d => d == null || d.Count <= 3)
                .WithMessage("A Sales Order can have at most 3 discounts.")
                .When(x => x.SalesOrderDetails != null);

            RuleFor(x => x.SalesOrderDetails!.Discounts)
                .Must(d => d == null || d.Select(x => x.SlabTypeId).Distinct().Count() == d.Count)
                .WithMessage("Each applied discount must have a unique SlabTypeId.")
                .When(x => x.SalesOrderDetails?.Discounts != null && x.SalesOrderDetails.Discounts.Any());

            RuleFor(x => x.SalesOrderDetails!.Discounts)
                .Must(d => d == null || d.Select(x => x.DiscountMasterId).Distinct().Count() == d.Count)
                .WithMessage("Each applied discount must reference a unique DiscountMaster.")
                .When(x => x.SalesOrderDetails?.Discounts != null && x.SalesOrderDetails.Discounts.Any());

            RuleForEach(x => x.SalesOrderDetails!.Discounts)
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
                .When(x => x.SalesOrderDetails?.Discounts != null && x.SalesOrderDetails.Discounts.Any());
        }
    }
}
