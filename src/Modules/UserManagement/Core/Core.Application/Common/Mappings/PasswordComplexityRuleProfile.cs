using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Domain.Entities;
using Core.Application.PwdComplexityRule.Queries;
using Core.Application.PwdComplexityRule.Commands;
using Core.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using Core.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using Core.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using static Core.Domain.Enums.Common.Enums;



namespace Core.Application.Common.Mappings
{
    public class PasswordComplexityRuleProfile :Profile
    {
        public PasswordComplexityRuleProfile()
        {
           
            CreateMap<CreatePasswordComplexityRuleCommand, Core.Domain.Entities.PasswordComplexityRule>()                      
            .ForMember(dest => dest.PwdComplexityRule, opt => opt.MapFrom(src => src.PwdComplexityRule))        
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

             CreateMap< Core.Domain.Entities.PasswordComplexityRule, PwdRuleDto>()             
           .ForMember(dest => dest.PwdComplexityRule, opt => opt.MapFrom(src => src.PwdComplexityRule));
       

            CreateMap<UpdatePasswordComplexityRuleCommand, Core.Domain.Entities.PasswordComplexityRule>()
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));     

            CreateMap <Core.Domain.Entities.PasswordComplexityRule , PwdRuleDto>();   
            // CreateMap<PwdRuleStatusDto, Core.Domain.Entities.PasswordComplexityRule>()  
            // .ForMember(dest => dest.Id, opt => opt.Ignore())
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ));

            CreateMap<Core.Domain.Entities.PasswordComplexityRule, GetPwdRuleDto>();
            CreateMap< Core.Domain.Entities.PasswordComplexityRule, PwdComplexityRuleAutoCompleteDto>();

            CreateMap<PwdRuleStatusDto, Core.Domain.Entities.PasswordComplexityRule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<Core.Domain.Entities.PasswordComplexityRule, PwdRuleDto>();

       // CreateMap<GetPwdRuleDto,Core.Domain.Entities.PasswordComplexityRule >(); 

             CreateMap<DeletePasswordComplexityRuleCommand, Core.Domain.Entities.PasswordComplexityRule>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

           
        }
}
}