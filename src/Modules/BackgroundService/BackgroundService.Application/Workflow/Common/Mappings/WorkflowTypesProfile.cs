using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Workflow.Common.Mappings
{
    public class WorkflowTypesProfile : Profile
    {
        public WorkflowTypesProfile()
        {
            CreateMap<WorkflowType, WorkflowTypeDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0));
            
           CreateMap<WorkflowType, GetWorkflowTypeAutoCompleteDto>(); 
           CreateMap<CreateWorkflowTypeCommand, WorkflowType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())               
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateWorkflowTypeCommand, WorkflowType>()    
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


              CreateMap<DeleteWorkflowTypeCommand, WorkflowType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    
        }
    }
}