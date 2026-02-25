using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Domain.Common;
using FluentValidation;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Presentation.Validation.PurchaseIndent
{
    public class UpdatePurchaseIndentCommandValidator : AbstractValidator<UpdatePurchaseIndentCommand>
    {
        private readonly IWorkflowLookup _workflowLookup;
        public UpdatePurchaseIndentCommandValidator(IWorkflowLookup workflowLookup)
        {
            _workflowLookup = workflowLookup;

            RuleFor(x => x.IndentDate)
                            .NotEmpty()
                            .NotNull()
                            .WithMessage("Indent Date is required.");

            RuleFor(x => x.IndentTypeId)
                            .NotEmpty()
                            .NotNull()
                            .WithMessage("Indent Type is required.");

            RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .NotNull()
                            .WithMessage("UnitId is required.");

            RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .NotNull()
                            .WithMessage("DepartmentId is required.");

           RuleFor(x => x.IsDraft)
                        .Must(v => v == 0 || v == 1) 
                        .WithMessage("IsDraft must be one of: 0, 1.");

            RuleFor(x => new { x.UnitId, x.DepartmentId })
                            .MustAsync(async (indent, cancellation) => 
                          await _workflowLookup.IsApproveWorkflowConfigureAsync(MiscEnumEntity.PurchaseIndent,indent.UnitId, indent.DepartmentId))
                            .WithMessage("Approval Workflow is not configured.");
        }
    }
}