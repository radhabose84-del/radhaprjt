using AutoMapper;
using UserManagement.Application.Location.Command.CreateLocation;
using UserManagement.Application.Location.Command.DeleteLocation;
using UserManagement.Application.Location.Command.UpdateLocation;
using UserManagement.Application.Location.Queries.GetAllLocation;
using UserManagement.Application.Location.Queries.GetLocationAutoSearch;
using UserManagement.Application.Location.Queries.GetLocationById;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<CreateLocationCommand, UserManagement.Domain.Entities.Location>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateLocationCommand, UserManagement.Domain.Entities.Location>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<DeleteLocationCommand, UserManagement.Domain.Entities.Location>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<UserManagement.Domain.Entities.Location, GetAllLocationDto>();
            CreateMap<UserManagement.Domain.Entities.Location, LocationByIdDto>();
            CreateMap<UserManagement.Domain.Entities.Location, LocationAutoCompleteDto>();
        }
    }
}
