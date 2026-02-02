using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Domain.Common;
using FluentValidation;
using PurchaseManagement.API.Validation.Common;

namespace PurchaseManagement.API.Validation.PurchaseIndent
{
    public class CreatePurchaseIndentCommandValidator : AbstractValidator<CreatePurchaseIndentCommand>
    {
        // private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowGrpcClient _workflowGrpcClient;
        public CreatePurchaseIndentCommandValidator(IWorkflowGrpcClient workflowGrpcClient)
        {
            _workflowGrpcClient = workflowGrpcClient;

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
                          await _workflowGrpcClient.IsApproveWorkflowConfigure(MiscEnumEntity.PurchaseIndent,indent.UnitId, indent.DepartmentId))
                            .WithMessage("Approval Workflow is not configured.");

          
        }
    }
}