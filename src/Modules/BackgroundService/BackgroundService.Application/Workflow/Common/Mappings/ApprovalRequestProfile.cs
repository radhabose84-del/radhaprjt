using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory;
using BackgroundService.Domain.Entities.Workflow;
using Contracts.Dtos.Purchase;

namespace BackgroundService.Application.Workflow.Common.Mappings
{
    public class ApprovalRequestProfile : Profile
    {
        public ApprovalRequestProfile()
        {



            CreateMap<ApprovalRequest, ApprovalRequestHeaderDto>()
          .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => src.Status.Code));

            CreateMap<ApprovalRequest, ApprovalRequestLineDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Code));

            CreateMap<ApproveApprovalRequestCommand, ApprovalRequest>()
            .ForMember(dest => dest.ApprovalRequestLines, opt => opt.MapFrom(src => src.ApprovalRequestLine))
            .ForMember(dest => dest.ApprovalDocuments, opt => opt.MapFrom(src => src.ApprovalDocument))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ApprovalRequestHeaderId));

            CreateMap<ApprovalDocumentDto, ApprovalDocument>();

            CreateMap<ApproveApprovalRequestLineDto, ApprovalRequestLine>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ApprovalRequestLineId))
            .ForMember(dest => dest.ApprovalRequestId, opt => opt.MapFrom(src => src.ApprovalRequestHeaderId));
            CreateMap<ApproveApprovalRequestLineDto, UpdateLineStatusDto>()
            .ForMember(dest => dest.ModuleLineId, opt => opt.MapFrom(src => src.ModuleLineTransactionId));

            CreateMap<ApproveApprovalRequestLineDto, ApproveLineStatusDto>()
            .ForMember(dest => dest.ApprovalRequestLineId, opt => opt.MapFrom(src => src.ApprovalRequestLineId))
            .ForMember(dest => dest.ModuleLineTransactionId, opt => opt.MapFrom(src => src.ModuleLineTransactionId))
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved));

            CreateMap<ApprovalRequest, ApprovedHistoryDto>()
            .ForMember(dest => dest.ApproverName, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedDate, opt => opt.MapFrom(src => src.ModifiedDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Code))
            .ForMember(dest => dest.StepOrder, opt => opt.MapFrom(src => src.ApprovalStepDetail.StepOrder));
            

            CreateMap<ApprovalRequest, ApprovalRequestWithLinesDto>()
            .ForMember(d => d.Lines,
                opt => opt.MapFrom(s => s.ApprovalRequestLines));

            CreateMap<ApprovalRequestLine, ApprovalRequestDto>();
        }
    }
}