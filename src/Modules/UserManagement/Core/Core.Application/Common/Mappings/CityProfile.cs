using AutoMapper;
using Core.Domain.Entities;
using Core.Application.City.Commands.CreateCity;
using Core.Application.City.Commands.UpdateCity;
using Core.Application.City.Commands.DeleteCity;
using static Core.Domain.Enums.Common.Enums;
using Core.Application.City.Queries.GetCities;

namespace Core.Application.Common.Mappings
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
