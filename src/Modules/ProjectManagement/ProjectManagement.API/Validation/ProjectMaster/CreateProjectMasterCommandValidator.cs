using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using Contracts.Interfaces.External.IWorkflow;
using ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster;
using FluentValidation;
using ProjectManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace ProjectManagement.API.Validation.ProjectMaster
{
    public class CreateProjectMasterCommandValidator : AbstractValidator<CreateProjectMasterCommand>
    {
        //  private readonly IWorkflowGrpcClient _workflowGrpcClient;
         
        private readonly List<ValidationRule> _validationRules;
        public CreateProjectMasterCommandValidator(
            // IWorkflowGrpcClient workflowGrpcClient,
         List<ValidationRule> validationRules)
        {
        //  _workflowGrpcClient = workflowGrpcClient;
            _validationRules = validationRules;


         // ✅ Only apply dynamic rules if any exist
        // foreach (var rule in _validationRules)
        // {
        //     switch (rule.Rule)
        //     {
        //         case "Workflow":
        //             RuleFor(x => x.Project.UnitId)
        //                 .MustAsync(async (command, unitId, cancellation) =>
        //                     await _workflowGrpcClient.IsApproveWorkflowConfigure(
        //                         ProjectManagement.Domain.Common.MiscEnumEntity.ProjectMaster,
        //                         unitId,
        //                         command.Project.DepartmentId))
        //                 .WithMessage(rule.Error);
        //             break;
        //     }
        // }

            // ✅ Your static rules stay as-is
            RuleFor(x => x.Project.ProjectName)
                .NotEmpty().WithMessage("Project Name is required.")
                .MaximumLength(200).WithMessage("Project Name cannot exceed 200 characters.");

            RuleFor(x => x.Project.ProjectTypeId)
                .GreaterThan(0).WithMessage("Project Type is required.");

            RuleFor(x => x.Project.UnitId)
                .GreaterThan(0).WithMessage("Unit is required.");

            RuleFor(x => x.Project.DepartmentId)
                .GreaterThan(0).WithMessage("Department is required.");

            RuleFor(x => x.Project.BudgetAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Budget Amount cannot be negative.");

            RuleFor(x => x.Project.BudgetYearId)
                .GreaterThan(0).WithMessage("Budget Year is required.");

            RuleFor(x => x.Project.CostCenterId)
                .GreaterThan(0).WithMessage("Cost Center is required.");

            RuleFor(x => x.Project.CurrencyId)
                .GreaterThan(0).WithMessage("Currency is required.");

            RuleFor(x => x.Project.ProjectCategoryId)
                .GreaterThan(0).WithMessage("Project Category is required.");

            RuleFor(x => x.Project.AssetGroupId)
                .GreaterThan(0).WithMessage("Asset Group is required.");

            RuleFor(x => x.Project.PurposeRemarks)
                .NotEmpty().WithMessage("Purpose / Remarks is required.")
                .MaximumLength(1000).WithMessage("Purpose / Remarks cannot exceed 1000 characters.");

            RuleFor(x => x.Project.StartDate)
                .NotEmpty().WithMessage("Start Date is required.");

            RuleFor(x => x.Project.EndDate)
                .NotEmpty().WithMessage("End Date is required.")
                .GreaterThanOrEqualTo(x => x.Project.StartDate)
                .WithMessage("End Date must be greater than or equal to Start Date.");
        }
    }
}