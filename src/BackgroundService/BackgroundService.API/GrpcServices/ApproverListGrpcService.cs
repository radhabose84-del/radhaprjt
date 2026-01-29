using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces.External.IUser;
using Grpc.Core;
using GrpcServices.BackgroundService.Line;

namespace BackgroundService.API.GrpcServices
{
    public class ApproverListGrpcService : ApproverService.ApproverServiceBase
    {
        private readonly IApprovalRequestGrpcQuery _approvalRequestGrpcQuery;
        private readonly IMapper _mapper;
        private readonly IMenuGrpcClient _menuGrpcClient;
        public ApproverListGrpcService(IApprovalRequestGrpcQuery approvalRequestGrpcQuery, IMapper mapper, IMenuGrpcClient menuGrpcClient)
        {
            _approvalRequestGrpcQuery = approvalRequestGrpcQuery;
            _mapper = mapper;
            _menuGrpcClient = menuGrpcClient;
        }
        public override async Task<ApproverListResponse> GetApprover(ApproverRequest request, ServerCallContext context)
        {

            var data = await _approvalRequestGrpcQuery.GetApproverListByWorkFlowTypeAsync(request.ModuleTypeName, request.ModuleTransactionIds);
            //  var ApprovalReqDto = _mapper.Map<List<ApprovalRequestLineDto>>(data);
            var response = new ApproverListResponse();


            foreach (var item in data)
            {
                response.Approvalstatus.Add(new ApproverListDto
                {

                    Status = item.Status?.ToString() ?? string.Empty,
                    ApproverBinding = item.ApproverBinding?.ToString() ?? string.Empty,
                    ApproverValue = item.ApproverValue?.ToString() ?? string.Empty,
                    ApprovalRequestId = item.Id,
                    ModuleTransactionId = item.ModuleTransactionId,
                    IsEdit = item.IsEdit
                   
                });
            }

            return response;

        }
            public override async Task<ApproveWorkFlowResponse> IsApproveWorkflowConfigure(ApproveWorkflowRequest request, ServerCallContext context)
        {
              var MenuId = await _menuGrpcClient.GetMenuByNameAsync(request.MenuName);

            var data = await _approvalRequestGrpcQuery.IsApproveWorkflowConfigure(MenuId,request.UnitId,request.DepartmentId);
            //  var ApprovalReqDto = _mapper.Map<List<ApprovalRequestLineDto>>(data);
            var response = new ApproveWorkFlowResponse();

            response.IsValid = data;
            return response;

        }
    }
}