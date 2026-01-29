using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Workflow.Common.Mappings
{
    public class ApprovalStepDetailProfile : Profile
    {
    public ApprovalStepDetailProfile()
    {
      CreateMap<ApprovalStepDetail, ApprovalStepDetailDto>()
      .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
      .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.WorkflowType.MenuId))
      .ForMember(dest => dest.ApprovalStepName, opt => opt.MapFrom(src => src.ApprovalStep.Code))
      .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType.Code));

      CreateMap<WorkflowType, WorkflowTypeApprovalStepDto>();
      CreateMap<Domain.Entities.Notification.MiscMaster, ApprovalStepDto>()
      .ForMember(dest => dest.StepName, opt => opt.MapFrom(src => src.Code));

      CreateMap<CreateApprovalStepDetailCommand, ApprovalStepDetail>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
            .ForMember(dest => dest.ApprovalStepUnitMappings, opt => opt.MapFrom(src => src.ApprovalStepUnitMappings))
        //  .ForMember(dest => dest.ApprovalRules, opt => opt.MapFrom(src => src.ApprovalRules))
        .ForMember(dest => dest.ApprovalStepDepartmentMappings, opt => opt.MapFrom(src => src.ApprovalStepDepartmentMappings));

      CreateMap<ApprovalStepUnitMappingDto, ApprovalStepUnitMapping>();

      CreateMap<ApprovalStepDepartmentMappingDto, ApprovalStepDepartmentMapping>();



      CreateMap<UpdateApprovalStepDetailCommand, ApprovalStepDetail>()
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
          .ForMember(dest => dest.ApprovalStepUnitMappings, opt => opt.MapFrom(src => src.ApprovalStepUnitMappings))
        //  .ForMember(dest => dest.RuleSkipApproverMappings, opt => opt.MapFrom(src => src.RuleSkipApproverMappings))
        .ForMember(dest => dest.ApprovalStepDepartmentMappings, opt => opt.MapFrom(src => src.ApprovalStepDepartmentMappings));

      CreateMap<ApprovalStepUnitMappingUpdateDto, ApprovalStepUnitMapping>();
      CreateMap<ApprovalStepDepartmentMappingUpdateDto, ApprovalStepDepartmentMapping>();

      CreateMap<DeleteApprovalStepDetailCommand, ApprovalStepDetail>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

      CreateMap<ApprovalStepDetail, ApprovalStepDetailByIdDto>()
      .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
      .ForMember(dest => dest.ApprovalStepUnitMappings, opt => opt.MapFrom(src => src.ApprovalStepUnitMappings))
      .ForMember(dest => dest.ApprovalStepDepartmentMappings, opt => opt.MapFrom(src => src.ApprovalStepDepartmentMappings))
      .ForMember(dest => dest.ApprovalRules, opt => opt.MapFrom(src => src.ApprovalRules))
      .ForMember(dest => dest.TargetValueId, opt => opt.MapFrom(src => Convert.ToInt32(src.TargetValueId)));

      CreateMap<ApprovalStepUnitMapping, ApprovalStepUnitMappingByIdDto>();
      CreateMap<ApprovalStepDepartmentMapping, ApprovalStepDepartmentMappingByIdDto>();
      CreateMap<ApprovalRule, ApprovalRuleByIdDto>()
      .ForMember(dest => dest.ApprovalRuleConditions, opt => opt.MapFrom(src => src.Conditions));
      CreateMap<ApprovalRuleCondition, ApprovalRuleConditionByIdDto>()
      .ForMember(dest => dest.Datafield, opt => opt.MapFrom(src => src.Field));
      CreateMap<ApprovalDataField, ApprovalDatafieldByIdDto>();

      CreateMap<ApprovalStepDetail, ApprovalStepDetailAutoCompleteDto>()
      .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.WorkflowType.MenuId))
      .ForMember(dest => dest.ApprovalStep, opt => opt.MapFrom(src => src.ApprovalStep.Code));
        }
    }
}