using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup;
using MaintenanceManagement.Application.MiscMaster.Command.CreateMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;



using MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using FluentValidation;
using MaintenanceManagement.API.Validation.CostCenter;
using MaintenanceManagement.API.Validation.WorkCenter;
using MaintenanceManagement.API.Validation.MachineGroup;
using MaintenanceManagement.API.Validation.MiscMaster;
using MaintenanceManagement.API.Validation.MiscTypeMaster;
using MaintenanceManagement.API.Validation.ShiftMaster;
using MaintenanceManagement.API.Validation.ShiftMasterDetail;
using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.API.Validation.MaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using MaintenanceManagement.API.Validation.MaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster;
using MaintenanceManagement.API.Validation.ActivityMaster;
using MaintenanceManagement.API.Validation.MachineMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.API.Validation.MachineGroupUser;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser;

using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.API.Validation.ActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.API.Validation.MaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MaintenanceManagement.API.Validation.WorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using MaintenanceManagement.API.Validation.MRS;
using MaintenanceManagement.API.Validation.PreventiveSchedulers;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using MaintenanceManagement.API.Validation.Power.FeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using MaintenanceManagement.API.Validation.Power.PowerConsumption;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using MaintenanceManagement.API.Validation.Power.Feeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.API.Validation.MachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.API.Validation.Power.GeneratorConsumption;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Command;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;
using Microsoft.Extensions.DependencyInjection;

namespace MaintenanceManagement.API.Validation.Common
{
    public class ValidationService
    {

        public void AddValidationServices(IServiceCollection services)
        {

            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreateMachineGroupCommand>, CreateMachineGroupCommandValidator>();
            services.AddScoped<IValidator<DeleteMachineGroupCommand>, DeleteMachineGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateMachineGroupCommand>, UpdateMachineGroupCommandValidator>();
            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();



            services.AddScoped<IValidator<CreateShiftMasterCommand>, CreateShiftMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateShiftMasterCommand>, UpdateShiftMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteShiftMasterCommand>, DeleteShiftMasterCommandValidator>();
            services.AddScoped<IValidator<CreateShiftMasterDetailCommand>, CreateShiftMasterDetailCommandValidator>();
            services.AddScoped<IValidator<UpdateShiftMasterDetailCommand>, UpdateShiftMasterDetailCommandValidator>();
            services.AddScoped<IValidator<DeleteShiftMasterDetailCommand>, DeleteShiftMasterDetailCommandValidator>();
            services.AddScoped<IValidator<CreateCostCenterCommand>, CreateCostCenterCommandValidator>();
            services.AddScoped<IValidator<UpdateCostCenterCommand>, UpdateCostCenterCommandValidator>();
            services.AddScoped<IValidator<DeleteCostCenterCommand>, DeleteCostCenterCommandValidator>();
            services.AddScoped<IValidator<CreateWorkCenterCommand>, CreateWorkCenterCommandValidator>();
            services.AddScoped<IValidator<UpdateWorkCenterCommand>, UpdateWorkCenterCommandValidator>();
            services.AddScoped<IValidator<DeleteWorkCenterCommand>, DeleteWorkCenterCommandValidator>();
            services.AddScoped<IValidator<CreateMaintenanceCategoryCommand>, CreateMaintenanceCategoryCommandValidator>();
            services.AddScoped<IValidator<UpdateMaintenanceCategoryCommand>, UpdateMaintenanceCategoryCommandValidator>();
            services.AddScoped<IValidator<DeleteMaintenanceCategoryCommand>, DeleteMaintenanceCategoryCommandValidator>();
            services.AddScoped<IValidator<CreateMaintenanceTypeCommand>, CreateMaintenanceTypeCommandValidator>();
            services.AddScoped<IValidator<UpdateMaintenanceTypeCommand>, UpdateMaintenanceTypeCommandValidator>();
            services.AddScoped<IValidator<DeleteMaintenanceTypeCommand>, DeleteMaintenanceTypeCommandValidator>();

            services.AddScoped<IValidator<CreateActivityMasterCommand>, CreateActivityMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMachineMasterCommand>, CreateMachineMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMachineMasterCommand>, UpdateMachineMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMachineMasterCommand>, DeleteMachineMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateActivityMasterCommand>, UpdateActivityMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMachineGroupUserCommand>, CreateMachineGroupUserCommandValidator>();
            services.AddScoped<IValidator<UpdateMachineGroupUserCommand>, UpdateMachineGroupUserCommandValidator>();
            services.AddScoped<IValidator<DeleteMachineGroupUserCommand>, DeleteMachineGroupUserCommandValidator>();


            services.AddScoped<IValidator<CreateWorkOrderCommand>, CreateWorkOrderCommandValidator>();
            services.AddScoped<IValidator<UpdateWorkOrderCommand>, UpdateWorkOrderCommandValidator>();
            services.AddScoped<IValidator<UploadFileWorkOrderCommand>, UploadWorkOrderCommandValidator>();
            services.AddScoped<IValidator<UploadFileItemCommand>, UploadItemCommandValidator>();

            services.AddScoped<IValidator<CreateActivityCheckListMasterCommand>, CreateActivityCheckListMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateActivityCheckListMasterCommand>, UpdateActivityCheckListMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteActivityCheckListMasterCommand>, DeleteActivityCheckListMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMaintenanceRequestCommand>, CreateMaintenanceRequestCommandValidator>();
            services.AddScoped<IValidator<UpdateMaintenanceRequestCommand>, UpdateMaintenanceRequestCommandValidator>();
            services.AddScoped<IValidator<UpdateWOScheduleCommand>, UpdateWOScheduleCommandValidator>();


            services.AddScoped<IValidator<CreateWOScheduleCommand>, CreateWOScheduleCommandValidator>();
            services.AddScoped<IValidator<CreateMRSCommand>, CreateMRSCommandValidator>();
            services.AddScoped<IValidator<CreatePreventiveSchedulerCommand>, CreatePreventiveSchedulerCommandValidator>();
            services.AddScoped<IValidator<UpdatePreventiveSchedulerCommand>, UpdatePreventiveSchedulerCommandValidator>();
            services.AddScoped<IValidator<DeletePreventiveSchedulerCommand>, DeletePreventiveSchedulerCommandValidator>();
            services.AddScoped<IValidator<ActiveInActivePreventiveCommand>, UpdateActiveStatusCommandValidator>();
            services.AddScoped<IValidator<RescheduleBulkImportCommand>, BulkImportPreventiveSchedulerCommandValidator>();
            services.AddScoped<IValidator<CreateFeederGroupCommand>, CreateFeederGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateFeederGroupCommand>, UpdateFeederGroupCommandValidator>();

            services.AddScoped<IValidator<DeleteFeederGroupCommand>, DeleteFeederGroupCommandValidator>();
            services.AddScoped<IValidator<CreateFeederCommand>, CreateFeederCommandValidator>();
            services.AddScoped<IValidator<UpdateFeederCommand>, UpdateFeederCommandValidator>();
            services.AddScoped<IValidator<DeleteFeederCommand>, DeleteFeederCommandValidator>();

            services.AddScoped<IValidator<CreatePowerConsumptionCommand>, CreatePowerConsumptionCommandValidator>();
            services.AddScoped<IValidator<CreateGeneratorConsumptionCommand>, CreateGeneratorConsumptionCommandValidator>();

            services.AddScoped<IValidator<MapMachineCommand>, MapMachineCommandValidator>();
            services.AddScoped<IValidator<MachineWiseFrequencyUpdateCommand>, MachineWiseFrequencyUpdateCommandValidator>();
            services.AddScoped<IValidator<CreateMachineSpecficationCommand>, CreateMachineSpecCommandValidator>();
            services.AddScoped<IValidator<DeleteMachineSpecficationCommand>, DeleteMachineSpecCommandValidator>();
            services.AddScoped<IValidator<UpdateMachineSpecficationCommand>, UpdateMachineSpecCommandValidator>();
            services.AddScoped<IValidator<UpdateWorkOrderRequestDateCommand>, UpdateWorkOrderRequestDateCommandValidator>();
        }  
    }
}