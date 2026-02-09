using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.ProjectMaster.Command.UpdateProjectMaster;
using FluentValidation;

namespace ProjectManagement.API.Validation.ProjectMaster
{
    public class UpdateProjectMasterCommandValidator : AbstractValidator<UpdateProjectMasterCommand>
    {
        public UpdateProjectMasterCommandValidator()
        {
            RuleFor(x => x.Project.Id)
                .GreaterThan(0).WithMessage("Id is required.");

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

            RuleFor(x => x.Project.EndDate)
                .GreaterThanOrEqualTo(x => x.Project.StartDate)
                .WithMessage("End Date must be greater than or equal to Start Date.");
        }
        
    }
}