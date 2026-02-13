using BudgetManagement.Presentation.Validation.BudgetRequest;
using BudgetManagement.Presentation.Validation.BudgetAllocation;
using BudgetManagement.Presentation.Validation.MiscMaster;
using BudgetManagement.Presentation.Validation.MiscTypeMaster;
using BudgetManagement.Presentation.Validation.BudgetGroup;   
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FluentValidation;
using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Command.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using Microsoft.Extensions.DependencyInjection;


namespace BudgetManagement.Presentation.Validation.Common
{
    public class ValidationService
    {

        public void AddValidationServices(IServiceCollection services)
        {

            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<CreateBudgetAllocationCommand>, CreateBudgetAllocationCommandValidator>();
            services.AddScoped<IValidator<UpdateBudgetRequestCommand>, UpdateBudgetRequestCommandValidator>();
            services.AddScoped<IValidator<CreateBudgetRequestCommand>, CreateBudgetRequestCommandValidator>();
            services.AddScoped<IValidator<CreateBudgetGroupCommand>, CreateBudgetGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateBudgetGroupCommand>, UpdateBudgetGroupCommandValidator>();
            services.AddScoped<IValidator<DeleteBudgetGroupCommand>, DeleteBudgetGroupCommandValidator>();           

        }
    }
}
