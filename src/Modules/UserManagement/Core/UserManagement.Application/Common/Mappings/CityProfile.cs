using AutoMapper;
using UserManagement.Domain.Entities;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Commands.DeleteCity;
using static UserManagement.Domain.Enums.Common.Enums;
using UserManagement.Application.City.Queries.GetCities;

namespace UserManagement.Application.Common.Mappings
{
    public class CityProfile : Profile
    {        
        public CityProfile()
        { 
            CreateMap<DeleteCityCommand, Cities>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
            
            CreateMap<CreateCityCommand, Cities>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateCityCommand, Cities>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));      
            CreateMap<Cities, CityAutoCompleteDTO>();                 
        }
    }
}    
