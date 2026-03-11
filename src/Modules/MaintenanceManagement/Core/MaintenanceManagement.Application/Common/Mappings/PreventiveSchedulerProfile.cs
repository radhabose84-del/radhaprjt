// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Maintenance.Preventive;
// using Contracts.Interfaces;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
// using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
// using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById;
// using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById;
// using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine;
// using MaintenanceManagement.Domain.Entities;
// using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
// using static MaintenanceManagement.Domain.Common.BaseEntity;

// namespace MaintenanceManagement.Application.Common.Mappings
// {
//     public class PreventiveSchedulerProfile : Profile
//     {

//         public PreventiveSchedulerProfile()
//         {
//             CreateMap<CreatePreventiveSchedulerCommand, PreventiveSchedulerHeader>()
//             .ForMember(dest => dest.PreventiveSchedulerActivities, opt => opt.MapFrom(src => src.Activity))
//             .ForMember(dest => dest.PreventiveSchedulerItems, opt => opt.MapFrom(src => src.Items))
//             .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
//             .ForMember(dest => dest.UnitId, opt => opt.Ignore())
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
//             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

//             CreateMap<PreventiveSchedulerDetail, ScheduleDetailSagaDto>()
//             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
//              .AfterMap((src, dest) =>
//                {
//                    var target = src.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                    var mins = (int)(target - DateTime.Now).TotalMinutes;
//                    dest.DelayInMinutes = mins < 0 ? 5 : mins;
//                });

//                CreateMap<PreventiveSchedulerDetail, RollbackScheduleDetailDto>()
//             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
//              .AfterMap((src, dest) =>
//                {
//                    var target = src.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                    var mins = (int)(target - DateTime.Now).TotalMinutes;
//                    dest.DelayInMinutes = mins < 0 ? 5 : mins;
//                });

//             CreateMap<PreventiveSchedulerActivityDto, PreventiveSchedulerActivity>();
//             CreateMap<PreventiveSchedulerItemsDto, PreventiveSchedulerItems>()
//             .ForMember(dest => dest.OldItemId, opt => opt.MapFrom(src => src.ItemId))
//             .ForMember(dest => dest.ItemId, opt => opt.Ignore());

//             CreateMap<UpdatePreventiveSchedulerCommand, PreventiveSchedulerHeader>()
//            .ForMember(dest => dest.PreventiveSchedulerActivities, opt => opt.MapFrom(src => src.Activity))
//             .ForMember(dest => dest.PreventiveSchedulerItems, opt => opt.MapFrom(src => src.Items));
//             // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

//             CreateMap<PreventiveSchedulerActivityUpdateDto, PreventiveSchedulerActivity>();
//             CreateMap<PreventiveSchedulerItemUpdateDto, PreventiveSchedulerItems>()
//             .ForMember(dest => dest.OldItemId, opt => opt.MapFrom(src => src.ItemId))
//              .ForMember(dest => dest.ItemId, opt => opt.Ignore());

//             CreateMap<DeletePreventiveSchedulerCommand, PreventiveSchedulerHeader>()
//             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

//             CreateMap<PreventiveSchedulerHeader, PreventiveSchedulerHdrByIdDto>()
//            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.PreventiveSchedulerActivities))
//            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PreventiveSchedulerItems))
//            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0));

//             CreateMap<PreventiveSchedulerActivity, PreventiveSchedulerActivityByIdDto>();
//             CreateMap<PreventiveSchedulerItems, PreventiveSchedulerItemByIdDto>()
//             .ForMember(dest => dest.OldItemId, opt => opt.MapFrom(src => src.OldItemId));

//             CreateMap<MaintenanceManagement.Domain.Entities.MachineMaster, PreventiveSchedulerDetail>()
//             .ForMember(dest => dest.MachineId, opt => opt.MapFrom(src => src.Id))
//             .ForMember(dest => dest.Id, opt => opt.Ignore())
//             .ForMember(dest => dest.PreventiveSchedulerHeaderId, opt => opt.Ignore())
//             .ForMember(dest => dest.WorkOrderCreationStartDate, opt => opt.Ignore())
//             .ForMember(dest => dest.ActualWorkOrderDate, opt => opt.Ignore())
//             .ForMember(dest => dest.MaterialReqStartDays, opt => opt.Ignore())
//             .ForMember(dest => dest.RescheduleReason, opt => opt.Ignore());

//             CreateMap<PreventiveSchedulerHeader, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>()
//             .ForMember(dest => dest.Id, opt => opt.Ignore())
//             .ForMember(dest => dest.PreventiveScheduleId, opt => opt.MapFrom((src, dest, destMember, ctx) =>
//                 ctx.Items.ContainsKey("PreventiveSchedulerDetailId") ? (int)ctx.Items["PreventiveSchedulerDetailId"] : 0))
//             .ForMember(dest => dest.StatusId, opt => opt.MapFrom((src, dest, destMember, ctx) =>
//                 ctx.Items.ContainsKey("StatusId") ? (int)ctx.Items["StatusId"] : 0))
//             // .ForMember(dest => dest.WorkOrderDocNo, opt => opt.MapFrom((src, dest, destMember, ctx) =>
//             //     ctx.Items.ContainsKey("WorkOrderDocNo")))
//             // .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => _ipAddressService.GetCompanyId() ?? 0))
//             // .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => _ipAddressService.GetUnitId() ?? 0))
//             .ForMember(dest => dest.WorkOrderActivities, opt => opt.MapFrom(src => src.PreventiveSchedulerActivities))
//             .ForMember(dest => dest.WorkOrderItems, opt => opt.MapFrom(src => src.PreventiveSchedulerItems))
//             .ForMember(dest => dest.DowntimeStart, opt => opt.Ignore())
//             .ForMember(dest => dest.DowntimeEnd, opt => opt.Ignore())
//             .ForMember(dest => dest.RootCauseId, opt => opt.Ignore())
//             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
//             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
//             .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
//             .ForMember(dest => dest.CreatedIP, opt => opt.Ignore())
//             .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
//             .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
//             .ForMember(dest => dest.ModifiedByName, opt => opt.Ignore())
//             .ForMember(dest => dest.ModifiedIP, opt => opt.Ignore());

//             CreateMap<PreventiveSchedulerActivity, WorkOrderActivity>()
//             .ForMember(dest => dest.Id, opt => opt.Ignore())
//             .ForMember(dest => dest.WorkOrderId, opt => opt.Ignore())
//             .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => src.ActivityId));

//             CreateMap<PreventiveSchedulerItems, WorkOrderItem>()
//           .ForMember(dest => dest.Id, opt => opt.Ignore())
//           .ForMember(dest => dest.WorkOrderId, opt => opt.Ignore())
//           // .ForMember(dest => dest.Sou, opt => opt.MapFrom(src => src.SourceId))
//           .ForMember(dest => dest.OldItemCode, opt => opt.MapFrom(src => src.OldItemId));
//             CreateMap<PreventiveSchedulerDetail, PreventiveSchedulerDetail>()
//             .ForMember(dest => dest.Id, opt => opt.Ignore());

//             CreateMap<ActiveInActivePreventiveCommand, PreventiveSchedulerHeader>()
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

//             CreateMap<PreventiveSchedulerHeaderBulkImportDto, PreventiveSchedulerHeader>()
//             .ForMember(dest => dest.PreventiveSchedulerDetails, opt => opt.MapFrom(src => src.PreventDetails))
//             .ForMember(dest => dest.PreventiveSchedulerActivities, opt => opt.MapFrom(src => src.PreventActivities))
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
//             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

//             CreateMap<PrevetiveSchedulerDetailBulkImportDto, PreventiveSchedulerDetail>()
//             .ForMember(dest => dest.PreventiveSchedulerHeaderId, opt => opt.Ignore())
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
//             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

//             CreateMap<PreventiveSchedulerBulkImprotActivityDto, PreventiveSchedulerActivity>()
//             .ForMember(dest => dest.PreventiveSchedulerHeaderId, opt => opt.Ignore());

//             CreateMap<PreventiveSchedulerHeader, PreventiveSchedulerDto>()
//             .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.MachineGroup.GroupName))
//            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.PreventiveSchedulerActivities))
//            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PreventiveSchedulerItems))
//            .ForMember(dest => dest.PreventiveSchedulerDtl, opt => opt.MapFrom(src => src.PreventiveSchedulerDetails));

//             CreateMap<PreventiveSchedulerActivity, MachineDetailActivityDto>()
//             .ForMember(dest => dest.ActivityName, opt => opt.MapFrom(src => src.Activity.ActivityName));
//             CreateMap<PreventiveSchedulerItems, MachineDetailItemsDto>()
//             .ForMember(dest => dest.OldItemId, opt => opt.MapFrom(src => src.OldItemId));

//             CreateMap<PreventiveSchedulerDetail, MachineDetailBySchedulerIdDto>()
//             .ForMember(dest => dest.MachineCode, opt => opt.MapFrom(src => src.Machine.MachineCode))
//             .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine.MachineName))
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0));

//             CreateMap<PreventiveSchedulerHeader, RollbackHeaderDto>()
//             .ForMember(dest => dest.rollbackActivities, opt => opt.MapFrom(src => src.PreventiveSchedulerActivities))
//            .ForMember(dest => dest.rollbackItems, opt => opt.MapFrom(src => src.PreventiveSchedulerItems));

//             CreateMap<PreventiveSchedulerActivity, RollbackActivityDto>()
//             .ForMember(dest => dest.Id, opt => opt.Ignore());
//             CreateMap<PreventiveSchedulerItems, RollbackItemsDto>()
//             .ForMember(dest => dest.Id, opt => opt.Ignore());

//             CreateMap<RollbackHeaderDto, PreventiveSchedulerHeader>()
//           .ForMember(dest => dest.PreventiveSchedulerActivities, opt => opt.MapFrom(src => src.rollbackActivities))
//          .ForMember(dest => dest.PreventiveSchedulerItems, opt => opt.MapFrom(src => src.rollbackItems));

//             CreateMap<RollbackActivityDto, PreventiveSchedulerActivity>();
//             CreateMap<RollbackItemsDto, PreventiveSchedulerItems>();
//             CreateMap<MaintenanceManagement.Domain.Entities.MachineMaster, UnMappedMachineDto>();

//             CreateMap<PreventiveSchedulerDetail, ScheduleDetailUpdateDto>()
//              .AfterMap((src, dest) =>
//               {
//                   var target = src.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
//                   var mins = (int)(target - DateTime.Now).TotalMinutes;
//                   dest.DelayInMinutes = mins < 0 ? 5 : mins;
//               });
//             CreateMap<ScheduleDetailSagaDto, PreventiveSchedulerDetail>();
//             CreateMap<RollbackScheduleDetailDto, PreventiveSchedulerDetail>();

//             CreateMap<MapPreventiveScheduleDetailDto, PreventiveSchedulerDetail>()
//             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
//             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
//         }
//     }
// }