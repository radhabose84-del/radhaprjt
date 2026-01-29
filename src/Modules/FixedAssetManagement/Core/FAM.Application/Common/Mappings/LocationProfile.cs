using AutoMapper;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Application.Location.Queries.GetLocations;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<CreateLocationCommand, FAM.Domain.Entities.Location>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateLocationCommand, FAM.Domain.Entities.Location>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteLocationCommand,  FAM.Domain.Entities.Location>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            
            CreateMap< FAM.Domain.Entities.Location, LocationDto>();            
            CreateMap< FAM.Domain.Entities.Location, LocationAutoCompleteDto>();
        }
        
    }
}