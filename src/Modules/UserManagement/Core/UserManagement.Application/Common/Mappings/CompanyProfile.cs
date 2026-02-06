using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Companies.Commands.CreateCompany;
using UserManagement.Application.Companies.Commands.DeleteCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            CreateMap<CompanyDTO, Company>()
                
                 .ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.CompanyAddress))
                 .ForMember(dest => dest.CompanyContact, opt => opt.MapFrom(src => src.CompanyContact))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                 .ForMember(dest => dest.PanNumber, opt => opt.MapFrom(src => src.PanNumber.Trim().ToUpperInvariant()));

                 CreateMap<UpdateCompanyDTO, Company>()
                
                 .ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.CompanyAddress))
                 .ForMember(dest => dest.CompanyContact, opt => opt.MapFrom(src => src.CompanyContact))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive))
                 .ForMember(dest => dest.PanNumber, opt => opt.MapFrom(src => src.PanNumber.Trim().ToUpperInvariant()));
                 
              CreateMap<CompanyAddressDTO, CompanyAddress>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
            .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.PinCode))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.AlternatePhone, opt => opt.MapFrom(src => src.AlternatePhone))
            .ReverseMap();

             CreateMap<CompanyContactDTO, CompanyContact>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Designation))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks))
            .ReverseMap();

            CreateMap<DeleteCompanyCommand, Company>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<Company, GetCompanyDTO>();
            // .ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.CompanyAddress))
            // .ForMember(dest => dest.CompanyContact, opt => opt.MapFrom(src => src.CompanyContact));
             CreateMap<Company, GetByIdDTO>()
            .ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.CompanyAddress))
            .ForMember(dest => dest.CompanyContact, opt => opt.MapFrom(src => src.CompanyContact));

            CreateMap<CompanyAddress, CompanyAddressDTO>()
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
            .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.PinCode))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.AlternatePhone, opt => opt.MapFrom(src => src.AlternatePhone));

            CreateMap<CompanyContact, CompanyContactDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Designation))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks));

            CreateMap<Company, CompanyAutoCompleteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName));
           
        
                 
        }
             
    }
}