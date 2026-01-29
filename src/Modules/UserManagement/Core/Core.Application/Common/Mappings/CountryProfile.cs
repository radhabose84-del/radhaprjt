using AutoMapper;
using Core.Application.Country.Commands.CreateCountry;
using Core.Application.Country.Queries.GetCountries;
using Core.Domain.Entities;
using Core.Application.Country.Commands.UpdateCountry;
using Core.Application.Country.Commands.DeleteCountry;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{    
        public class CountryProfile  : Profile
    {        
        public CountryProfile()
        {
            CreateMap<DeleteCountryCommand, Countries>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
                  
             CreateMap<CreateCountryCommand, Countries>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateCountryCommand, Countries>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));      
            CreateMap<Countries, CountryAutoCompleteDTO>();    

        }
    }
}    
