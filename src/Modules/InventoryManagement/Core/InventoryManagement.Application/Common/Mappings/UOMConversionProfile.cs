using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class UOMConversionProfile : Profile
    {
        public UOMConversionProfile()
        {


            CreateMap<InventoryManagement.Domain.Entities.UOMConversion, UOMConversionDto>();

            CreateMap<CreateUOMConversionCommand, InventoryManagement.Domain.Entities.UOMConversion>()
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateUOMConversionCommand, InventoryManagement.Domain.Entities.UOMConversion>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteUOMConversionCommand, InventoryManagement.Domain.Entities.UOMConversion>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

        }
        
    }
}