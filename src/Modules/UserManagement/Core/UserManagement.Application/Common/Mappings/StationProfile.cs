using AutoMapper;
using UserManagement.Application.Station.Command.CreateStation;
using UserManagement.Application.Station.Command.DeleteStation;
using UserManagement.Application.Station.Command.UpdateStation;
using UserManagement.Application.Station.Queries.GetAllStation;
using UserManagement.Application.Station.Queries.GetStationAutoSearch;
using UserManagement.Application.Station.Queries.GetStationById;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class StationProfile : Profile
    {
        public StationProfile()
        {
            CreateMap<CreateStationCommand, UserManagement.Domain.Entities.Station>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateStationCommand, UserManagement.Domain.Entities.Station>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<DeleteStationCommand, UserManagement.Domain.Entities.Station>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<UserManagement.Domain.Entities.Station, GetAllStationDto>();
            CreateMap<UserManagement.Domain.Entities.Station, StationByIdDto>();
            CreateMap<UserManagement.Domain.Entities.Station, StationAutoCompleteDto>();
        }
    }
}
