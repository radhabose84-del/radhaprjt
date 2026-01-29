using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Grpc.Core;
using GrpcServices.BackgroundService.Line;

namespace BackgroundService.API.GrpcServices
{
    public class ApprovalLineRequestStatusGrpcService : ApprovalRequestLineStatusService.ApprovalRequestLineStatusServiceBase
    {
        private readonly IApprovalRequestGrpcQuery _approvalRequestGrpcQuery;
        private readonly IMapper _mapper;
        public ApprovalLineRequestStatusGrpcService(IApprovalRequestGrpcQuery approvalRequestGrpcQuery, IMapper mapper)
        {
            _approvalRequestGrpcQuery = approvalRequestGrpcQuery;
            _mapper = mapper;
        }
        public override async Task<ApprovalLineStatusListResponse> GetApprovalRequestLineStatus(ApprovalStatusRequest request, ServerCallContext context)
        {

            var data = await _approvalRequestGrpcQuery.ApprovalRequestLineStatusByWorkFlowType(request.ModuleTypeName, request.ModuleTransactionIds, request.UserId);

            var response = new ApprovalLineStatusListResponse();

            foreach (var item in data)
            {
                response.Approvalstatus.Add(new ApprovalRequestLineStatusDto
                {

                    ModuleLineTransactionId = Convert.ToInt32(item.ModuleLineTransactionId),
                    Status = item.Status?.ToString() ?? string.Empty
                });
            }

            return response;

        }
          public override async Task<ApprovalLineStatusListResponse> GetApprovalRequestLine(ApprovalStatusRequest request, ServerCallContext context)
        {

            var data = await _approvalRequestGrpcQuery.ApprovalRequestHeaderStatusByWorkFlowType(request.ModuleTypeName,request.ModuleTransactionIds,request.UserId);
             
            var response = new ApprovalLineStatusListResponse();
            
            foreach (var item in data)
              {
                  response.Approvalstatus.Add(new ApprovalRequestLineStatusDto
                  {
                      
                      ModuleLineTransactionId = Convert.ToInt32(item.ModuleLineTransactionId),
                      ApprovalRequestLineTransactionId = item.Id
                  });
              }
            
              return response;

        }
    }
}