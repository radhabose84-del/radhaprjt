using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Domain.Entities.MRS;
using static InventoryManagement.Application.MRS.Command.CreateMrsEntry.CreateMrsEntryDto;
using static InventoryManagement.Application.MRS.Command.UpdateMrsEntry.UpdateMrsEntryDto;

namespace InventoryManagement.Application.Common.Mappings.MRS
{
    public class MrsEntryProfile : Profile
    {
         public MrsEntryProfile()
        {
            CreateMap<CreateMrsEntryDto, MrsHeader>()
                .ForMember(dest => dest.MrsDetailHeaderName, opt => opt.MapFrom(src => src.MrsDetails))
                .ForMember(dest => dest.MrsNo, opt => opt.Ignore())           // Generated later
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CreateMrsDetailDto, MrsDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MrsHeaderDetails, opt => opt.Ignore());
               


            // ---------------- UPDATE MAPPINGS ----------------
            CreateMap<UpdateMrsEntryDto, MrsHeader>()
                .ForMember(dest => dest.MrsDetailHeaderName, opt => opt.MapFrom(src => src.UpdateMrsDetails))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StatusId, opt => opt.Ignore()) // status may be managed separately
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.MrsNo, opt => opt.Ignore());

            CreateMap<UpdateMrsDetailDto, MrsDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // handled if existing/new
                .ForMember(dest => dest.MrsHeaderDetails, opt => opt.Ignore());
               

          CreateMap<MrsHeader, MrsReverseMapDto>()
                    .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src))
                    .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.MrsDetailHeaderName));
                CreateMap<MrsHeader, CreateMrsEntryDto>()
                    .ForMember(dest => dest.MrsDetails, opt => opt.MapFrom(src => src.MrsDetailHeaderName));
                CreateMap<MrsDetail, CreateMrsDetailDto>();
        }
    }
}