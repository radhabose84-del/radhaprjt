using AutoMapper;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.Location.Queries.GetSubLocations;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class SubLocationProfile : Profile
    {
        public SubLocationProfile()
        {
            CreateMap<CreateSubLocationCommand, FAM.Domain.Entities.SubLocation>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSubLocationCommand, FAM.Domain.Entities.SubLocation>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteSubLocationCommand,  FAM.Domain.Entities.SubLocation>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            
            CreateMap< FAM.Domain.Entities.SubLocation, SubLocationDto>();            
            CreateMap< FAM.Domain.Entities.SubLocation, SubLocationAutoCompleteDto>();
        }
        
    }
}