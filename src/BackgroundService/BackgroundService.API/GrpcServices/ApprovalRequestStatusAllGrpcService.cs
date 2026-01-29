using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Grpc.Core;
using GrpcServices.BackgroundService;

namespace BackgroundService.API.GrpcServices
{
    public class ApprovalRequestStatusAllGrpcService : ApprovalRequestStatusAllService.ApprovalRequestStatusAllServiceBase
    {
        private readonly IApprovalRequestGrpcQuery _approvalRequestQuery;
        private readonly IMapper _mapper;
        public ApprovalRequestStatusAllGrpcService(IApprovalRequestGrpcQuery approvalRequestQuery, IMapper mapper)
        {
            _approvalRequestQuery = approvalRequestQuery;
            _mapper = mapper;
        }
        public override async Task<ApprovalStatusAllListResponse> GetApprovalRequestStatusAll(ApprovalStatusRequest request, ServerCallContext context)
        {

            var data = await _approvalRequestQuery.GetApprovalRequestByWorkFlowTypeAsync(request.ModuleTypeName);
             var ApprovalReqDto = _mapper.Map<List<ApprovalRequestHeaderDto>>(data);
            var response = new ApprovalStatusAllListResponse();
            
            foreach (var item in ApprovalReqDto)
              {
                  response.Approvalstatus.Add(new ApprovalRequestStatusDto
                  {
                      ModuleTransactionId = Convert.ToInt32(item.ModuleTransactionId),
                      CurrentStatus = item.CurrentStatus?.ToString() ?? string.Empty
                  });
              }
            
              return response;

        }
    }
}