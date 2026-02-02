using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using static PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry.CreateGRNEntryDto;
using static PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry.UpdateGRNEntryDto;

namespace PurchaseManagement.Application.Common.Mappings.GRN.GRNEntry
{
    public class GRNEntryProfile : Profile
    {
        public GRNEntryProfile()
        {
           // Map header, ignore GrnDetails (we'll handle manually)
        CreateMap<CreateGRNEntryDto, GrnHeader>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsGrnGenerated, opt => opt.MapFrom(src => src.IsGrnGenerated == 1))
            .ForMember(dest => dest.IsQcApproved, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.GrnDetails, opt => opt.Ignore()); // ignore

        // Map detail if needed (optional, mostly for simple properties)
        CreateMap<CreateGRNDetailsDto, GrnDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GrnId, opt => opt.Ignore());

            CreateMap<UpdateGRNEntryDto, GrnHeader>()
            .ForMember(dest => dest.IsGrnGenerated, opt => opt.MapFrom(src => src.IsGrnGenerated == 1 ? true : false))
            .ForMember(dest => dest.IsQcApproved, opt => opt.MapFrom(src => src.IsQcApproved == 1 ? true : false))
            //.ForMember(dest => dest.GrnDetails, opt => opt.MapFrom(src => src.UpdateGRNDetailsDtos));
            .ForMember(dest => dest.GrnDetails, opt => opt.Ignore());
            CreateMap<UpdateGRNDetailsDto, GrnDetail>();

            CreateMap<CreateGRNPutawayDto, GrnPutAwayRule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // Map standard fields
            .ForMember(dest => dest.GrnId, opt => opt.MapFrom(src => src.GrnId))
            .ForMember(dest => dest.GrnDetailId, opt => opt.MapFrom(src => src.GrnDetailId))
            .ForMember(dest => dest.PoId, opt => opt.MapFrom(src => src.PoId))
            .ForMember(dest => dest.PoSlNoLocal, opt => opt.MapFrom(src => src.PoSlNoLocal))
            .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.ItemId))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
            .ForMember(dest => dest.QcAcceptedQtyPurchaseUom, opt => opt.MapFrom(src => src.QcAcceptedQtyPurchaseUom))
            .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.WarehouseId))
            .ForMember(dest => dest.StorageTypeId, opt => opt.MapFrom(src => src.StorageTypeId))
            .ForMember(dest => dest.TargetId, opt => opt.MapFrom(src => src.TargetId))
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.PriorityId))
            .ForMember(dest => dest.PurchaseUomId, opt => opt.MapFrom(src => src.PurchaseUomId))
            .ForMember(dest => dest.StockUomId, opt => opt.MapFrom(src => src.StockUomId))
            .ForMember(dest => dest.ConversionFactor, opt => opt.MapFrom(src => src.ConversionFactor))
            .ForMember(dest => dest.QcAcceptedQtyStockUom, opt => opt.MapFrom(src => src.QcAcceptedQtyStockUom))
            .ForMember(dest => dest.Override, opt => opt.MapFrom(src => src.Override == 1 ? true : false))
              // Optionally map audit fields if present in DTO
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedIP, opt => opt.Ignore())
            
            // Optionally set PutAwayDate if you want to default to now
            .ForMember(dest => dest.PutAwayDate, opt => opt.Ignore());




        }
    }
}