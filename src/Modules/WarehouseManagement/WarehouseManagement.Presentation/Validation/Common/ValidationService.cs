using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using FluentValidation;
using WarehouseManagement.Presentation.Validation.BinMaster;
using WarehouseManagement.Presentation.Validation.RackMaster;
using WarehouseManagement.Presentation.Validation.WarehouseMaster;
using WarehouseManagement.Infrastructure.Repositories.BinMaster;
using Microsoft.Extensions.DependencyInjection;

namespace WarehouseManagement.Presentation.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreateWarehouseMasterCommand>, CreateWarehouseMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateWarehouseMasterCommand>, UpdateWarehouseMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteWarehouseMasterCommand>, DeleteWareMasterCommandValidator>();
            services.AddScoped<IValidator<CreateRackMasterCommand>, CreateRackMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateRackMasterCommand>, UpdateRackMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteRackMasterCommand>, DeleteRackMasterCommandValidator>();
            services.AddScoped<IValidator<CreateBinMasterCommand>, CreateBinMasterCommandValidator>();
            
        }
    }
}