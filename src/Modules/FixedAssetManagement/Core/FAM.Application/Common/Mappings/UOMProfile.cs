using AutoMapper;
using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Command.DeleteUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class UOMProfile : Profile
    {    
        public UOMProfile()
        {
            CreateMap<CreateUOMCommand, FAM.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateUOMCommand, FAM.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteUOMCommand,  FAM.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            CreateMap<Domain.Entities.UOM, UOMDto>();
            CreateMap<Domain.Entities.UOM, UOMAutoCompleteDto>();
            // CreateMap<Domain.Entities.UOM, UOMTypeAutoCompleteDto>();

    //         CreateMap<Domain.Entities.UOM, UOMTypeAutoCompleteDto>()
    // .ForMember(dest => dest.UOMType, opt => opt.MapFrom(src => src.UOMType.Code));


        }
        
    }
}