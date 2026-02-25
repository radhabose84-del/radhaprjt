using FluentValidation;
using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Presentation.Validation.MiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Presentation.Validation.MiscMaster;
using ProjectManagement.Presentation.Validation.ProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectManagement.Presentation.Validation.Common
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
            // services.AddScoped<IValidator<CreateProjectMasterCommand>, CreateProjectMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateProjectMasterCommand>, UpdateProjectMasterCommandValidator>();
            services.AddScoped<IValidator<CreateProjectWorkBreakdownStructureCommand>, CreateProjectWorkBreakdownStructureCommandValidator>();
            services.AddScoped<IValidator<UpdateProjectWorkBreakdownStructureCommand>, UpdateProjectWorkBreakdownStructureCommandValidator>();
            services.AddScoped<IValidator<DeleteProjectWorkBreakdownStructureCommand>, DeleteProjectWorkBreakdownStructureCommandValidator>();


        }
    }
}