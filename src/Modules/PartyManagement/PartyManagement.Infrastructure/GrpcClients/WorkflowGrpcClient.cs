using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IWorkflow;
using Grpc.Core;
using GrpcServices.BackgroundService;
using GrpcServices.BackgroundService.Line;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.Infrastructure.GrpcClients
{
    public class WorkflowGrpcClient : IWorkflowGrpcClient
    {
        private readonly ApprovalRequestStatusAllService.ApprovalRequestStatusAllServiceClient _approvalRequestStatusAllService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApprovalRequestLineStatusService.ApprovalRequestLineStatusServiceClient _approvalRequestLineStatusService;
        private readonly ApproverService.ApproverServiceClient _approverServiceClient;
        public WorkflowGrpcClient(ApprovalRequestStatusAllService.ApprovalRequestStatusAllServiceClient approvalRequestStatusAllService,
        IHttpContextAccessor httpContextAccessor, ApprovalRequestLineStatusService.ApprovalRequestLineStatusServiceClient approvalRequestLineStatusService,
        ApproverService.ApproverServiceClient approverServiceClient)
        {
            _approvalRequestStatusAllService = approvalRequestStatusAllService;
            _httpContextAccessor = httpContextAccessor;
            _approvalRequestLineStatusService = approvalRequestLineStatusService;
            _approverServiceClient = approverServiceClient;
        }
        public async Task<List<Contracts.Dtos.Workflow.ApprovalRequestStatusDto>> GetAllApprovalRequestStatusAsync(string ModuleTypeName)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };
            var request = new GrpcServices.BackgroundService.ApprovalStatusRequest { ModuleTypeName = ModuleTypeName };

            var response = await _approvalRequestStatusAllService.GetApprovalRequestStatusAllAsync(request, new CallOptions(metadata));

            return response.Approvalstatus.Select(u => new Contracts.Dtos.Workflow.ApprovalRequestStatusDto
            {
                ModuleTransactionId = u.ModuleTransactionId,
                CurrentStatus = u.CurrentStatus
            }).ToList();
        }
        public async Task<List<Contracts.Dtos.Workflow.ApprovalRequestLineStatusDto>> GetApprovalRequestLineAsync(string ModuleTypeName, IEnumerable<int> ModuleTransactionIds, int UserId)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };
            var request = new GrpcServices.BackgroundService.Line.ApprovalStatusRequest { ModuleTypeName = ModuleTypeName };

            request.ModuleTransactionIds.AddRange(ModuleTransactionIds?.Distinct() ?? Enumerable.Empty<int>());
            request.UserId = UserId;

            var response = await _approvalRequestLineStatusService.GetApprovalRequestLineAsync(request, new CallOptions(metadata));

            return response.Approvalstatus.Select(u => new Contracts.Dtos.Workflow.ApprovalRequestLineStatusDto
            {
                ModuleLineTransactionId = u.ModuleLineTransactionId,
                ApprovalRequestLineTransactionId = u.ApprovalRequestLineTransactionId
            }).ToList();
        }

        public async Task<List<Contracts.Dtos.Workflow.ApprovalRequestLineStatusDto>> GetApprovalRequestLineStatusAsync(string ModuleTypeName, IEnumerable<int> ModuleTransactionIds, int UserId)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };
            var request = new GrpcServices.BackgroundService.Line.ApprovalStatusRequest { ModuleTypeName = ModuleTypeName };

            request.ModuleTransactionIds.AddRange(ModuleTransactionIds?.Distinct() ?? Enumerable.Empty<int>());
            request.UserId = UserId;

            var response = await _approvalRequestLineStatusService.GetApprovalRequestLineStatusAsync(request, new CallOptions(metadata));

            return response.Approvalstatus.Select(u => new Contracts.Dtos.Workflow.ApprovalRequestLineStatusDto
            {
                ModuleLineTransactionId = u.ModuleLineTransactionId,
                Status = u.Status
            }).ToList();
        }
        public async Task<List<Contracts.Dtos.Workflow.ApproverListDto>> GetApproverListAsync(string ModuleTypeName,IEnumerable<int> ModuleTransactionIds)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };
             var request = new GrpcServices.BackgroundService.Line.ApproverRequest { };
             request.ModuleTypeName = ModuleTypeName;
             request.ModuleTransactionIds.AddRange(ModuleTransactionIds?.Distinct() ?? Enumerable.Empty<int>());
            var response = await _approverServiceClient.GetApproverAsync(request, new CallOptions(metadata));

              return response.Approvalstatus.Select(u => new Contracts.Dtos.Workflow.ApproverListDto
              {

                  Status = u.Status,
                  ApproverBinding = u.ApproverBinding,
                  ApproverValue = u.ApproverValue,
                  ApprovalRequestId = u.ApprovalRequestId,
                  ModuleTransactionId = u.ModuleTransactionId
            }).ToList();
         
        }

        public async Task<bool> IsApproveWorkflowConfigure(string ModuleTypeName, int UnitId, int DepartmentId)
        {
             var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };
             var request = new GrpcServices.BackgroundService.Line.ApproveWorkflowRequest { };
             request.MenuName = ModuleTypeName;
            request.UnitId = UnitId;
            request.DepartmentId = DepartmentId;
            var response = await _approverServiceClient.IsApproveWorkflowConfigureAsync(request, new CallOptions(metadata));

              return response.IsValid;
        }
    }
}