using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.AdminSecuritySettings.Queries;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Application.FinancialYear.Command.DeleteFinancialYear;
using UserManagement.Application.FinancialYear.Command.UpdateFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using static UserManagement.Domain.Enums.Common.Enums;
using static UserManagement.Domain.Enums.FinancialYearEnum;


namespace UserManagement.Application.Common.Mappings
{
   

    public class FinancialYearProfile :Profile
    {
         public FinancialYearProfile()
        {
             CreateMap<UserManagement.Domain.Entities.FinancialYear, GetFinancialYearDto>();

            CreateMap<UserManagement.Domain.Entities.FinancialYear, GetFinancialYearAutoCompleteDto>();
               


             CreateMap<CreateFinancialYearCommand, UserManagement.Domain.Entities.FinancialYear>()
             .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
              .ForMember(dest => dest.FinYearName,opt => opt.MapFrom(src => src.FinYearName))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

               CreateMap<UserManagement.Domain.Entities.FinancialYear ,FinancialYearDto >()
              .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate)) 
              .ForMember(dest => dest.FinYearName,opt => opt.MapFrom(src => src.FinYearName));  
          
               
              CreateMap<UpdateFinancialYearCommand, UserManagement.Domain.Entities.FinancialYear>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (FinancialYearStatus)src.IsActive)); 

              CreateMap<DeleteFinancialYearCommand, UserManagement.Domain.Entities.FinancialYear>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

               



                 
        

        }
    }
}