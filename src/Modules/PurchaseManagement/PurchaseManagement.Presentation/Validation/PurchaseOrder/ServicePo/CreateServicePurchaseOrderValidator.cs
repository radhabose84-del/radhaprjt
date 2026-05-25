using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using FluentValidation;
using Shared.Validation.Common;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Interfaces.Validations.MaintenanceManagement;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    public class CreateServicePurchaseOrderValidator : AbstractValidator<CreateServicePurchaseOrderDto>
    {

        private readonly IWorkflowLookup _workflowLookup;
        private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceRequestValidation _maintenanceRequestValidation;


        public CreateServicePurchaseOrderValidator(
            IWorkflowLookup workflowLookup,
            List<ValidationRule> validationRules,
            IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository,
            IMaintenanceRequestValidation maintenanceRequestValidation)
        {
            _workflowLookup = workflowLookup;
            _validationRules = validationRules;
            _maintenanceRequestValidation = maintenanceRequestValidation;

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                   case "Workflow":
                                    RuleFor(x => x.UnitId)
                                .MustAsync(async (unitId, cancellation) =>
                                    await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                        PurchaseManagement.Domain.Common.MiscEnumEntity.ServicePO, // entity type
                                        unitId,
                                        0))                      // DepartmentId not required, pass null
                                .WithMessage(rule.Error);
                            break;
                }
            }

            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.PODate).NotEmpty();
            RuleFor(x => x.VendorId).GreaterThan(0);
            RuleForEach(x => x.ServicePos).SetValidator(new ServiceHeaderValidator());

            // ───────────────────────────────────────────────────────────────
            //  External Maintenance Request linkage rules (per ESR story)
            //  Rule 1: Each linked request must be eligible for Service PO linkage
            //  Rule 2: All linked requests share the same vendor as the PO header
            //  Rule 3: No duplicate RequestId across lines of the same PO
            // ───────────────────────────────────────────────────────────────
            RuleFor(x => x).CustomAsync(async (dto, ctx, ct) =>
            {
                if (dto.ServicePos == null) return;

                var linkedRequestIds = dto.ServicePos
                    .SelectMany(sh => sh.Lines ?? new List<PurchaseOrderServiceLineDto>())
                    .Where(l => l.RequestId.HasValue && l.RequestId.Value > 0)
                    .Select(l => l.RequestId!.Value)
                    .ToList();

                if (linkedRequestIds.Count == 0) return;

                var distinctIds = linkedRequestIds.Distinct().ToList();

                // Rule 1 — eligibility (status / type / active / not deleted)
                foreach (var rid in distinctIds)
                {
                    var available = await _maintenanceRequestValidation
                        .IsAvailableForServicePoAsync(rid, ct);
                    if (!available)
                    {
                        ctx.AddFailure(
                            $"MaintenanceRequest {rid} is not available for Service PO linkage " +
                            "(must be External, Active, and in status Open, InProgress or PartiallyConverted).");
                    }
                }

                // Rule 2 — same-vendor consistency
                foreach (var rid in distinctIds)
                {
                    var refData = await _maintenanceRequestValidation.GetRefAsync(rid, ct);
                    if (refData?.VendorId.HasValue == true && refData.VendorId.Value != dto.VendorId)
                    {
                        ctx.AddFailure(
                            $"MaintenanceRequest {rid} is for vendor {refData.VendorId.Value} " +
                            $"but the PO header vendor is {dto.VendorId}. All linked requests must share the same vendor.");
                    }
                }

                // Rule 3 — no duplicate RequestId across lines of the same PO
                var dupes = linkedRequestIds
                    .GroupBy(id => id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                foreach (var dupId in dupes)
                {
                    ctx.AddFailure(
                        $"MaintenanceRequest {dupId} is linked to more than one line in this PO. " +
                        "Each request may appear at most once.");
                }
            });
        }

        private class ServiceHeaderValidator : AbstractValidator<PurchaseOrderServiceHeaderDto>
        {
            public ServiceHeaderValidator()
            {
                RuleFor(x => x.ServiceCategoryId).GreaterThan(0);
                RuleForEach(x => x.Lines).SetValidator(new ServiceLineValidator());
            }
        }

        private class ServiceLineValidator : AbstractValidator<PurchaseOrderServiceLineDto>
        {
            public ServiceLineValidator()
            {
                RuleFor(x => x.PlannedQuantity).GreaterThan(0);
                RuleFor(x => x.PlannedRate).GreaterThan(0);
            }
        }
    }
}