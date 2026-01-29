using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetAllApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Workflow.Common.Mappings
{
    public class ApprovalRuleProfile : Profile
    {
        public ApprovalRuleProfile()
        {
            CreateMap<ApprovalRule, ApprovalRuleDto>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
           .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action.Code))
           .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.ApprovalStepDetail.WorkflowType.MenuId));

            CreateMap<WorkflowType, WorkflowTypeDto>();
            CreateMap<CreateApprovalRuleCommand, ApprovalRule>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                 .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.ApprovalRuleConditions)); ;

            CreateMap<ApprovalRuleConditionDto, ApprovalRuleCondition>()
            .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Datafield));

            CreateMap<ApprovalDatafieldDto, ApprovalDataField>();

            CreateMap<UpdateApprovalRuleCommand, ApprovalRule>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.ApprovalRuleConditions));

            CreateMap<ApprovalRuleConditionUpdateDto, ApprovalRuleCondition>()
       .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Datafield));

            CreateMap<ApprovalDatafieldUpdateDto, ApprovalDataField>();


            CreateMap<DeleteApprovalRuleCommand, ApprovalRule>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
                
                     CreateMap<ApprovalRule, ApprovalRuleByIdDto>()
            .ForMember(dest => dest.ApprovalRuleConditions, opt => opt.MapFrom(src => src.Conditions));
            CreateMap<ApprovalRuleCondition, ApprovalRuleConditionByIdDto>()
            .ForMember(dest => dest.Datafield, opt => opt.MapFrom(src => src.Field));
            CreateMap<ApprovalDataField, ApprovalDatafieldByIdDto>();
        }
    }
}