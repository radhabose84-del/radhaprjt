using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.AdminSecuritySettings.Queries;
using Core.Application.FinancialYear.Command.CreateFinancialYear;
using Core.Application.FinancialYear.Command.DeleteFinancialYear;
using Core.Application.FinancialYear.Command.UpdateFinancialYear;
using Core.Application.FinancialYear.Queries.GetFinancialYear;
using Core.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using Core.Domain.Entities;
using Core.Domain.Enums;
using static Core.Domain.Enums.Common.Enums;
using static Core.Domain.Enums.FinancialYearEnum;


namespace Core.Application.Common.Mappings
{
   

    public class FinancialYearProfile :Profile
    {
         public FinancialYearProfile()
        {
             CreateMap<Core.Domain.Entities.FinancialYear, GetFinancialYearDto>();

            CreateMap<Core.Domain.Entities.FinancialYear, GetFinancialYearAutoCompleteDto>();
               


             CreateMap<CreateFinancialYearCommand, Core.Domain.Entities.FinancialYear>()
             .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
              .ForMember(dest => dest.FinYearName,opt => opt.MapFrom(src => src.FinYearName))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

               CreateMap<Core.Domain.Entities.FinancialYear ,FinancialYearDto >()
              .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartYear))
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate)) 
              .ForMember(dest => dest.FinYearName,opt => opt.MapFrom(src => src.FinYearName));  
          
               
              CreateMap<UpdateFinancialYearCommand, Core.Domain.Entities.FinancialYear>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (FinancialYearStatus)src.IsActive)); 

              CreateMap<DeleteFinancialYearCommand, Core.Domain.Entities.FinancialYear>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

               



                 
        

        }
    }
}