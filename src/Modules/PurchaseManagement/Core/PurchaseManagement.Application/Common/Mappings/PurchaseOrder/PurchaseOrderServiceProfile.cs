using AutoMapper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseOrder
{
    public class PurchaseOrderServiceProfile : Profile
    {

        public PurchaseOrderServiceProfile()
        {

            // Header -> CreateServicePurchaseOrderDto
            CreateMap<PurchaseOrderHeader, CreateServicePurchaseOrderDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ServicePos, o => o.MapFrom(s => s.ServicePos))
                .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms));

            // Service header: Entity.Items -> DTO.Lines
            CreateMap<PurchaseOrderServiceHeader, PurchaseOrderServiceHeaderDto>()
                .ForMember(d => d.Lines, o => o.MapFrom(s => s.Items));

            // Line: Entity.PurchaseOrderServiceSchedules -> DTO.Schedules
            CreateMap<PurchaseOrderServiceLine, PurchaseOrderServiceLineDto>()
                .ForMember(d => d.Schedules, o => o.MapFrom(s => s.PurchaseOrderServiceSchedules));

            // Leaves
            CreateMap<PurchaseOrderServiceSchedule, PurchaseOrderServiceScheduleDto>();
            CreateMap<PurchasePaymentTerm, PurchaseOrderServicePaymentTermDto>();
            CreateMap<PurchaseOrderServiceDetailDto, CreateServicePurchaseOrderDto>()
            .ForMember(d => d.ServicePos,   o => o.MapFrom(s => s.ServicePo))
            .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
            .ForMember(d => d.Documents,    o => o.MapFrom(s => s.DocumentsList));

        // 🔹 NEW: DocumentsList (ServiceDocumentDto) -> Documents (PurchaseDocumentDto)
        CreateMap<ServiceDocumentDto, PurchaseServiceDocumentDto>();
            // // Header (root)          

            // 6) PO -> workflow envelope (service)
            CreateMap<PurchaseOrderHeader, CreatePOServiceReverseDto>()
            .ForMember(d => d.Header, o => o.MapFrom(s => new POServiceWorkFlowDto
            {
                Id       = s.Id,
                PONumber = s.PONumber,
                VendorId = s.VendorId,
                StatusId = s.StatusId,
                UnitId   = s.UnitId
            }))
            .ForMember(d => d.Lines, o => o.MapFrom(s =>
                (s.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
                    .Select(h => new POServiceWorkFlowDto
                    {
                        Id       = h.Id,
                        PONumber = s.PONumber,
                        VendorId = s.VendorId,
                        StatusId = s.StatusId,
                        UnitId   = s.UnitId
                    })
                    .ToList()
            ));

            CreateMap<CreateServicePurchaseOrderDto, CreatePOServiceReverseDto>()
            .ForMember(d => d.Header, o => o.MapFrom(s => new POServiceWorkFlowDto
            {
                Id       = s.Id,
                PONumber = s.PONumber,
                VendorId = s.VendorId,
                StatusId = s.StatusId,
                UnitId   = s.UnitId
            }))
            .ForMember(d => d.Lines, o => o.MapFrom(s =>
                (s.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeaderDto>())
                    .Where(h => h.Id.HasValue && h.Id.Value > 0)
                    .Select(h => new POServiceWorkFlowDto
                    {
                        Id       = h.Id!.Value,
                        PONumber = s.PONumber,
                        VendorId = s.VendorId,
                        StatusId = s.StatusId,
                        UnitId   = s.UnitId
                    })
                    .ToList()
            ));
          
            // ===== UPDATE =====
            CreateMap<ServicePurchaseOrderUpdateDto, PurchaseOrderHeader>()
                .ForMember(d => d.PaymentTerms,
                    o => o.MapFrom(s => s.PaymentTerms))
                .ForMember(d => d.ServicePos,
                    o => o.MapFrom(s => s.ServicePos));
                    

                        CreateMap<ServicePurchaseOrderUpdateDto, PurchaseOrderHeader>()
                .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
                .ForMember(d => d.ServicePos,   o => o.MapFrom(s => s.ServicePos))
                // ignore server/audit fields (set in repo)
                .ForMember(d => d.OldPOId,        o => o.Ignore())
                .ForMember(d => d.IsActive,       o => o.Ignore())
                .ForMember(d => d.IsDeleted,      o => o.Ignore())
                .ForMember(d => d.CreatedBy,      o => o.Ignore())
                .ForMember(d => d.CreatedByName,  o => o.Ignore())
                .ForMember(d => d.CreatedIP,      o => o.Ignore())
                .ForMember(d => d.CreatedDate,    o => o.Ignore())
                .ForMember(d => d.ModifiedBy,     o => o.Ignore())
                .ForMember(d => d.ModifiedByName, o => o.Ignore())
                .ForMember(d => d.ModifiedIP,     o => o.Ignore())
                .ForMember(d => d.ModifiedDate,   o => o.Ignore());

            // ===== add: CreateServicePurchaseOrderDto -> PurchaseOrderHeader (so both DTOs work) =====
            CreateMap<CreateServicePurchaseOrderDto, PurchaseOrderHeader>()
                .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
                .ForMember(d => d.ServicePos,   o => o.MapFrom(s => s.ServicePos))
                // ignore server/audit fields (set in repo)
                .ForMember(d => d.OldPOId,        o => o.Ignore())
                .ForMember(d => d.IsActive,       o => o.Ignore())
                .ForMember(d => d.IsDeleted,      o => o.Ignore())
                .ForMember(d => d.CreatedBy,      o => o.Ignore())
                .ForMember(d => d.CreatedByName,  o => o.Ignore())
                .ForMember(d => d.CreatedIP,      o => o.Ignore())
                .ForMember(d => d.CreatedDate,    o => o.Ignore())
                .ForMember(d => d.ModifiedBy,     o => o.Ignore())
                .ForMember(d => d.ModifiedByName, o => o.Ignore())
                .ForMember(d => d.ModifiedIP,     o => o.Ignore())
                .ForMember(d => d.ModifiedDate,   o => o.Ignore());

            // ===== nested DTO -> Entity maps (needed for full graph) =====
            CreateMap<PurchaseOrderServiceHeaderDto, PurchaseOrderServiceHeader>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id ?? 0))
                .ForMember(d => d.PurchaseOrderId, o => o.MapFrom(s => s.PurchaseOrderId))
                .ForMember(d => d.ServiceCategoryId, o => o.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.ContractTypeId, o => o.MapFrom(s => s.ContractTypeId))
                .ForMember(d => d.FrequencyId, o => o.MapFrom(s => s.FrequencyId))
                .ForMember(d => d.ValidityFrom, o => o.MapFrom(s => s.ValidityFrom))
                .ForMember(d => d.ValidityTo, o => o.MapFrom(s => s.ValidityTo))
                .ForMember(d => d.TotalOccurrences, o => o.MapFrom(s => s.TotalOccurrences))
                .ForMember(d => d.OverallLimit, o => o.MapFrom(s => s.OverallLimit))
                .ForMember(d => d.TermsId, o => o.MapFrom(s => s.TermsId))
                .ForMember(d => d.CostCenterId, o => o.MapFrom(s => s.CostCenterId))
                .ForMember(d => d.ModeOfDispatchId, o => o.MapFrom(s => s.ModeOfDispatchId))
                .ForMember(d => d.FreightCharges, o => o.MapFrom(s => s.FreightCharges ?? 0))
                .ForMember(d => d.TermDescription, o => o.MapFrom(s => s.TermDescription))
                .ForMember(d => d.DeliveryAddress, o => o.MapFrom(s => s.DeliveryAddress))
                .ForMember(d => d.BillingAddress, o => o.MapFrom(s => s.BillingAddress))
                .ForMember(d => d.POImage,    o => o.MapFrom(s => s.POImage))
                // Lines -> Items
                .ForMember(d => d.Items,             o => o.MapFrom(s => s.Lines))
                // ignore server/audit flags
                .ForMember(d => d.IsActive,          o => o.Ignore())
                .ForMember(d => d.IsDeleted,         o => o.Ignore())
                .ForMember(d => d.CreatedBy,         o => o.Ignore())
                .ForMember(d => d.CreatedByName,     o => o.Ignore())
                .ForMember(d => d.CreatedIP,         o => o.Ignore())
                .ForMember(d => d.CreatedDate,       o => o.Ignore())
                .ForMember(d => d.ModifiedBy,        o => o.Ignore())
                .ForMember(d => d.ModifiedByName,    o => o.Ignore())
                .ForMember(d => d.ModifiedIP,        o => o.Ignore())
                .ForMember(d => d.ModifiedDate,      o => o.Ignore());

            CreateMap<PurchaseOrderServiceLineDto, PurchaseOrderServiceLine>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id ?? 0))
                .ForMember(d => d.PurchaseOrderId, o => o.MapFrom(s => s.PurchaseOrderId))
                .ForMember(d => d.ServicePoHeaderId, o => o.MapFrom(s => s.ServicePoHeaderId))
                .ForMember(d => d.LineNo, o => o.MapFrom(s => s.LineNo))
                .ForMember(d => d.RequestId, o => o.MapFrom(s => s.RequestId))
                .ForMember(d => d.ServiceId, o => o.MapFrom(s => s.ServiceId))
                .ForMember(d => d.ServiceDescription, o => o.MapFrom(s => s.ServiceDescription))
                .ForMember(d => d.UOMId, o => o.MapFrom(s => s.UOMId))
                .ForMember(d => d.PlannedQuantity, o => o.MapFrom(s => s.PlannedQuantity))
                .ForMember(d => d.PlannedRate, o => o.MapFrom(s => s.PlannedRate))
                .ForMember(d => d.PlannedValue, o => o.MapFrom(s => s.PlannedValue))
                // string code vs FK id – ignore here; resolve separately if needed

                .ForMember(d => d.DiscountId,               o => o.MapFrom(s => s.DiscountId))                
                .ForMember(d => d.Discount,                 o => o.MapFrom(s => s.Discount))
                .ForMember(d => d.ItemCost,                 o => o.MapFrom(s => s.ItemCost))
                .ForMember(d => d.OtherCost,                o => o.MapFrom(s => s.OtherCost))
                .ForMember(d => d.OtherCharges,             o => o.MapFrom(s => s.OtherCharges))
                .ForMember(d => d.GstPercent,               o => o.MapFrom(s => s.GstPercent))
                .ForMember(d => d.Remarks,                  o => o.MapFrom(s => s.Remarks))
                // Schedules -> PurchaseOrderServiceSchedules
                .ForMember(d => d.PurchaseOrderServiceSchedules, o => o.MapFrom(s => s.Schedules))
                // ignore server/audit
                .ForMember(d => d.IsActive,                 o => o.Ignore())
                .ForMember(d => d.IsDeleted,                o => o.Ignore())
                .ForMember(d => d.CreatedBy,                o => o.Ignore())
                .ForMember(d => d.CreatedByName,            o => o.Ignore())
                .ForMember(d => d.CreatedIP,                o => o.Ignore())
                .ForMember(d => d.CreatedDate,              o => o.Ignore())
                .ForMember(d => d.ModifiedBy,               o => o.Ignore())
                .ForMember(d => d.ModifiedByName,           o => o.Ignore())
                .ForMember(d => d.ModifiedIP,               o => o.Ignore())
                .ForMember(d => d.ModifiedDate,             o => o.Ignore());

            CreateMap<PurchaseOrderServiceScheduleDto, PurchaseOrderServiceSchedule>()
                .ForMember(d => d.Id,                   o => o.MapFrom(s => s.Id ?? 0))
                .ForMember(d => d.PurchaseOrderId,      o => o.MapFrom(s => s.PurchaseOrderId))
                .ForMember(d => d.ServicePoHeaderId,    o => o.MapFrom(s => s.ServicePoHeaderId))
                .ForMember(d => d.ServiceItemId,        o => o.MapFrom(s => s.ServiceItemId))
                .ForMember(d => d.ScheduleNo,           o => o.MapFrom(s => s.ScheduleNo))
                .ForMember(d => d.OccurrencePeriod,     o => o.MapFrom(s => s.OccurrencePeriod))
                .ForMember(d => d.OccurrenceDescription, o => o.MapFrom(s => s.OccurrenceDescription))
                .ForMember(d => d.ActivityTypeId,       o => o.MapFrom(s => s.ActivityTypeId))
                .ForMember(d => d.PlannedDurationHrs,   o => o.MapFrom(s => s.PlannedDurationHrs))
                .ForMember(d => d.DueDate,              o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.ServiceStartDate,     o => o.MapFrom(s => s.ServiceStartDate))
                .ForMember(d => d.ServiceEndDate,       o => o.MapFrom(s => s.ServiceEndDate))
                .ForMember(d => d.PlannedQuantity,      o => o.MapFrom(s => s.PlannedQuantity))
                .ForMember(d => d.PlannedRate,          o => o.MapFrom(s => s.PlannedRate))
                .ForMember(d => d.PlannedValue,         o => o.MapFrom(s => s.PlannedValue))
                .ForMember(d => d.AutoGenerateSES,      o => o.MapFrom(s => s.AutoGenerateSES))
                .ForMember(d => d.Remarks,              o => o.MapFrom(s => s.Remarks))
                // ignore server/audit
                .ForMember(d => d.IsActive,             o => o.Ignore())
                .ForMember(d => d.IsDeleted,            o => o.Ignore())
                .ForMember(d => d.CreatedBy,            o => o.Ignore())
                .ForMember(d => d.CreatedByName,        o => o.Ignore())
                .ForMember(d => d.CreatedIP,            o => o.Ignore())
                .ForMember(d => d.CreatedDate,          o => o.Ignore())
                .ForMember(d => d.ModifiedBy,           o => o.Ignore())
                .ForMember(d => d.ModifiedByName,       o => o.Ignore())
                .ForMember(d => d.ModifiedIP,           o => o.Ignore())
                .ForMember(d => d.ModifiedDate,         o => o.Ignore());

            CreateMap<PurchaseOrderServicePaymentTermDto, PurchasePaymentTerm>()
                .ForMember(d => d.Id,              o => o.MapFrom(s => s.Id ?? 0))
                .ForMember(d => d.PurchaseOrderId, o => o.MapFrom(s => s.PurchaseOrderId))
                .ForMember(d => d.PaymentTermId,   o => o.MapFrom(s => s.PaymentTermId))
                .ForMember(d => d.AdvancePercent,  o => o.MapFrom(s => s.AdvancePercent))
                .ForMember(d => d.CreditDays,      o => o.MapFrom(s => s.CreditDays))
                .ForMember(d => d.PaymentModelId,  o => o.MapFrom(s => s.PaymentModelId))
                .ForMember(d => d.InsuranceId,     o => o.MapFrom(s => s.InsuranceId))
                .ForMember(d => d.InsurancePercent,o => o.MapFrom(s => s.InsurancePercent))
                .ForMember(d => d.InsuranceAmount, o => o.MapFrom(s => s.InsuranceAmount))
                .ForMember(d => d.AdvanceAmount,   o => o.MapFrom(s => s.AdvanceAmount))
                .ForMember(d => d.BalancePercent,  o => o.MapFrom(s => s.BalancePercent))
                .ForMember(d => d.BalanceAmount,   o => o.MapFrom(s => s.BalanceAmount))
                // ignore server/audit
                .ForMember(d => d.IsActive,        o => o.Ignore())
                .ForMember(d => d.IsDeleted,       o => o.Ignore())
                .ForMember(d => d.CreatedBy,       o => o.Ignore())
                .ForMember(d => d.CreatedByName,   o => o.Ignore())
                .ForMember(d => d.CreatedIP,       o => o.Ignore())
                .ForMember(d => d.CreatedDate,     o => o.Ignore())
                .ForMember(d => d.ModifiedBy,      o => o.Ignore())
                .ForMember(d => d.ModifiedByName,  o => o.Ignore())
                .ForMember(d => d.ModifiedIP,      o => o.Ignore())
                .ForMember(d => d.ModifiedDate,    o => o.Ignore());

         
           

        }
        
    }
}