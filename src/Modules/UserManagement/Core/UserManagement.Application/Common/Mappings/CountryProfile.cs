using AutoMapper;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Domain.Entities;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
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
