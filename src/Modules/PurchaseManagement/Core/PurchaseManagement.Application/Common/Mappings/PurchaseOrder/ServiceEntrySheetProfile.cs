using AutoMapper;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseOrder
{
    public class ServiceEntrySheetProfile : Profile
    {
        public ServiceEntrySheetProfile()
        {
            CreateMap<CreateServiceSheetDto, ServiceEntrySheet>()
            // normalize: prefer FK and resolve other values at query-time
                .ForMember(d => d.Id,       opt => opt.Ignore())   // don't overwrite PK
                .ForMember(d => d.StatusId, opt => opt.Ignore())  
                .ForMember(d => d.PurchaseOrderId, m => m.MapFrom(s => s.PurchaseOrderId))
                .ForMember(d => d.UnitId, m => m.MapFrom(s => s.UnitId))
                .ForMember(d => d.SESDate, m => m.MapFrom(s => s.SESDate))
                .ForMember(d => d.SESStatusId, m => m.MapFrom(s => s.SESStatusId))
                .ForMember(d => d.AttachmentFileName, m => m.MapFrom(s => s.AttachmentFileName))
                // optional domain fields
                .ForMember(d => d.ServiceCategoryId, m => m.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.ContractTypeId, m => m.MapFrom(s => s.ContractTypeId))
                .ForMember(d => d.ValidityFrom, m => m.MapFrom(s => s.ValidityFrom))
                .ForMember(d => d.ValidityTo, m => m.MapFrom(s => s.ValidityTo))
                // line-level primary service snapshot (if you keep it on header)
                .ForMember(d => d.ServiceId, m => m.MapFrom(s => s.ServiceId))
                .ForMember(d => d.ServiceDescription, m => m.MapFrom(s => s.ServiceDescription))
                .ForMember(d => d.ScheduleId, m => m.MapFrom(s => s.ScheduleID)) // handle naming
                .ForMember(d => d.OccurrenceNo, m => m.MapFrom(s => s.OccurrenceNo))
                .ForMember(d => d.OccurrencePeriod, m => m.MapFrom(s => s.OccurrencePeriod))
                .ForMember(d => d.ScheduleStartDate, m => m.MapFrom(s => s.ScheduleStartDate))
                .ForMember(d => d.ScheduleEndDate, m => m.MapFrom(s => s.ScheduleEndDate))
                .ForMember(d => d.WorkStartDate, m => m.MapFrom(s => s.WorkStartDate))
                .ForMember(d => d.WorkEndDate, m => m.MapFrom(s => s.WorkEndDate))
                .ForMember(d => d.DurationHrs, m => m.MapFrom(s => s.DurationHrs))
                .ForMember(d => d.LineRemarks, m => m.MapFrom(s => s.LineRemarks))
            //   .ForMember(d => d.StatusId, m => m.MapFrom(s => s.StatusId))
                // monetary (computed later if missing)
                .ForMember(d => d.ActualQuantity, m => m.MapFrom(s => s.ActualQuantity))
                .ForMember(d => d.ActualRate, m => m.MapFrom(s => s.ActualRate))
                .ForMember(d => d.ActualValue, m => m.MapFrom(s => s.ActualValue))
                .ForMember(d => d.DiscountTypeId, m => m.MapFrom(s => s.DiscountTypeId))
                .ForMember(d => d.DiscountValue, m => m.MapFrom(s => s.DiscountValue))
                .ForMember(d => d.TaxPercentage, m => m.MapFrom(s => s.TaxPercentage))
                .ForMember(d => d.TaxValue, m => m.MapFrom(s => s.TaxValue))
                .ForMember(d => d.TotalValue, m => m.MapFrom(s => s.TotalValue))
                // children
                .ForMember(d => d.Activities, m => m.MapFrom(s => s.Activities));

            CreateMap<CreateServiceSheetDto.CreateServiceEntryActivityDto, ServiceEntryActivity>()
                .ForMember(d => d.Id,  opt => opt.Ignore())
                .ForMember(d => d.ActivityTypeId, m => m.MapFrom(s => s.ActivityTypeId))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description))                
                .ForMember(d => d.PerformedByName, m => m.MapFrom(s => s.PerformedByName))
                .ForMember(d => d.SESActivityStatusId, m => m.MapFrom(s => s.SESActivityStatusId))
                .ForMember(d => d.StatusRemarks, m => m.MapFrom(s => s.StatusRemarks));

            CreateMap<ServiceEntrySheet, PoServiceHeaderByIdDto>(); 

            CreateMap<SesFromScheduleRawDto, CreateServiceSheetDto>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.SESDate, o => o.MapFrom(_ => DateTimeOffset.Now))
                .ForMember(d => d.SESStatusId, o => o.MapFrom(_ => 1103)) // Draft
                .ForMember(d => d.ScheduleID, o => o.MapFrom(s => s.ScheduleId))
                .ForMember(d => d.TaxPercentage, o => o.MapFrom(s => s.GstPercent)) // if GST drives tax
                .ForMember(d => d.ActualValue, o => o.Ignore())
                .ForMember(d => d.TaxValue, o => o.Ignore())
                .ForMember(d => d.TotalValue, o => o.Ignore())
                .ForMember(d => d.WorkStartDate, o => o.Ignore())
                .ForMember(d => d.WorkEndDate, o => o.Ignore())
                .ForMember(d => d.DurationHrs, o => o.Ignore())
                .ForMember(d => d.Activities, o => o.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.WorkStartDate ??= dest.ScheduleStartDate;
                    dest.WorkEndDate ??= dest.ScheduleEndDate;
                    dest.Activities ??= new List<CreateServiceSheetDto.CreateServiceEntryActivityDto>();
                });
             // Mapper for gedbyid
            CreateMap<ServiceEntryActivity, ServiceEntryActivityDto>();

            CreateMap<ServiceEntrySheet, GetServiceEntrySheetDto>()
            // if your entity uses ScheduleId (not ScheduleID), you can omit this
                .ForMember(d => d.ScheduleId, o => o.MapFrom(s => s.ScheduleId))
                .ForMember(d => d.Activities, o => o.MapFrom(s => s.Activities));    
      
        
         // SES → workflow DTO (single)
           CreateMap<ServiceEntrySheet, ServiceEntrySheetWorkFlowDto>();

        // SES aggregate → wrapper DTO
           CreateMap<ServiceEntrySheet, CreateServiceEntrySheetWorkflowDto>()
                .ForMember(d => d.Header, opt => opt.MapFrom(s => s))
                // Option 1: keep Lines empty
                .ForMember(d => d.Lines, opt => opt.Ignore());

        } 
    }
}