using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Domain.Entities;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Application.PwdComplexityRule.Commands;
using UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using static UserManagement.Domain.Enums.Common.Enums;



namespace UserManagement.Application.Common.Mappings
{
    public class PasswordComplexityRuleProfile :Profile
    {
        public PasswordComplexityRuleProfile()
        {
           
            CreateMap<CreatePasswordComplexityRuleCommand, UserManagement.Domain.Entities.PasswordComplexityRule>()                      
            .ForMember(dest => dest.PwdComplexityRule, opt => opt.MapFrom(src => src.PwdComplexityRule))        
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

             CreateMap< UserManagement.Domain.Entities.PasswordComplexityRule, PwdRuleDto>()             
           .ForMember(dest => dest.PwdComplexityRule, opt => opt.MapFrom(src => src.PwdComplexityRule));
       

            CreateMap<UpdatePasswordComplexityRuleCommand, UserManagement.Domain.Entities.PasswordComplexityRule>()
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));     

            CreateMap <UserManagement.Domain.Entities.PasswordComplexityRule , PwdRuleDto>();   
            // CreateMap<PwdRuleStatusDto, Core.Domain.Entities.PasswordComplexityRule>()  
            // .ForMember(dest => dest.Id, opt => opt.Ignore())
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ));

            CreateMap<UserManagement.Domain.Entities.PasswordComplexityRule, GetPwdRuleDto>();
            CreateMap< UserManagement.Domain.Entities.PasswordComplexityRule, PwdComplexityRuleAutoCompleteDto>();

            CreateMap<PwdRuleStatusDto, UserManagement.Domain.Entities.PasswordComplexityRule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<UserManagement.Domain.Entities.PasswordComplexityRule, PwdRuleDto>();

       // CreateMap<GetPwdRuleDto,Core.Domain.Entities.PasswordComplexityRule >(); 

             CreateMap<DeletePasswordComplexityRuleCommand, UserManagement.Domain.Entities.PasswordComplexityRule>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

           
        }
}
}