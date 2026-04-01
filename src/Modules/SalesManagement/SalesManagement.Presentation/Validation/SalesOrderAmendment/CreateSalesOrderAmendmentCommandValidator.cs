using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrderAmendment
{
    public class CreateSalesOrderAmendmentCommandValidator
        : AbstractValidator<CreateSalesOrderAmendmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderAmendmentQueryRepository _queryRepo;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;

        public CreateSalesOrderAmendmentCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderAmendmentQueryRepository queryRepo,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService)
        {
            _queryRepo = queryRepo;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;

            var maxLengthReason = maxLengthProvider.GetMaxLength<SalesOrderAmendmentHeader>("Reason") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.SalesOrderHeaderId)
                            .GreaterThan(0)
                            .WithMessage($"SalesOrderHeaderId {rule.Error}");

                        RuleFor(x => x.Reason)
                            .NotNull().WithMessage($"Reason {rule.Error}")
                            .NotEmpty().WithMessage($"Reason {rule.Error}");

                        RuleFor(x => x.AmendmentDetails)
                            .NotNull().WithMessage($"AmendmentDetails {rule.Error}")
                            .Must(d => d != null && d.Count > 0)
                            .WithMessage("At least one amendment detail line is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Reason)
                            .MaximumLength(maxLengthReason)
                            .WithMessage($"Reason {rule.Error} {maxLengthReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
                        break;

                    case "FKColumnDelete":
                        // E1 + E2: SO must exist and be Approved
                        RuleFor(x => x.SalesOrderHeaderId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepo.SalesOrderExistsAndApprovedAsync(id))
                            .WithMessage("Only approved sales orders can be amended.")
                            .When(x => x.SalesOrderHeaderId > 0);

                        // E3: No Dispatch Advice
                        RuleFor(x => x.SalesOrderHeaderId)
                            .MustAsync(async (id, ct) =>
                                !await _queryRepo.HasDispatchAdviceAsync(id))
                            .WithMessage("Amendment not allowed - dispatch advice already created.")
                            .When(x => x.SalesOrderHeaderId > 0);

                        // E5: No pending amendment
                        RuleFor(x => x.SalesOrderHeaderId)
                            .MustAsync(async (id, ct) =>
                                !await _queryRepo.HasPendingAmendmentAsync(id))
                            .WithMessage("An amendment is already pending approval for this order.")
                            .When(x => x.SalesOrderHeaderId > 0);
                        break;

                    case "GreaterThan":
                        // ChangeType is auto-derived in handler: any New* value → Modified, all null → Removed
                        // Validator only checks that New* values (if provided) are valid
                        RuleForEach(x => x.AmendmentDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.SalesOrderDetailId)
                                    .GreaterThan(0)
                                    .WithMessage($"SalesOrderDetailId {rule.Error}");

                                detail.RuleFor(d => d.NewQtyInBags)
                                    .GreaterThan(0)
                                    .WithMessage($"NewQtyInBags {rule.Error}")
                                    .When(d => d.NewQtyInBags.HasValue);

                                detail.RuleFor(d => d.NewExMillRate)
                                    .GreaterThan(0)
                                    .WithMessage($"NewExMillRate {rule.Error}")
                                    .When(d => d.NewExMillRate.HasValue);
                            })
                            .When(x => x.AmendmentDetails != null && x.AmendmentDetails.Count > 0);
                        break;

                    

                    case "Workflow":
                        RuleFor(x => x.AmendmentDetails)
                            .MustAsync(async (details, cancellation) =>
                            {
                                var orderUnitId = _ipAddressService.GetUnitId() ?? 0;
                                if (orderUnitId <= 0) return false;
                                return await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                    MiscEnumEntity.TransactionTypeSalesOrderAmendment,
                                    orderUnitId,
                                    0);
                            })
                            .WithMessage(rule.Error)
                            .When(x => x.AmendmentDetails != null);
                        break;

                    default:
                        break;
                }
            }

            // No duplicate SalesOrderDetailId within the same amendment
            RuleFor(x => x.AmendmentDetails)
                .Must(details =>
                {
                    if (details == null) return true;
                    var ids = details.Select(d => d.SalesOrderDetailId).ToList();
                    return ids.Distinct().Count() == ids.Count;
                })
                .WithMessage("Duplicate SalesOrderDetailId found in amendment details.")
                .When(x => x.AmendmentDetails != null && x.AmendmentDetails.Count > 0);
        }
    }
}
