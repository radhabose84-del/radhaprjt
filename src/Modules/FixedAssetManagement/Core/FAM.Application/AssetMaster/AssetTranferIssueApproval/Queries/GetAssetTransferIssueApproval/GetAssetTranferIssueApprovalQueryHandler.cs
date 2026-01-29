using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval
{
    public class GetAssetTranferIssueApprovalQueryHandler : IRequestHandler<GetAssetTranferIssueApprovalQuery,  ApiResponseDTO<List<AssetTransferIssueApprovalDto>>>
    {
        private readonly IAssetTransferIssueApprovalQueryRepository _assetTransferIssueQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 
        // private readonly IDepartmentGrpcClient _departmentGrpcClient;

        public GetAssetTranferIssueApprovalQueryHandler(IAssetTransferIssueApprovalQueryRepository assetTransferIssueQueryRepository, IMapper mapper, IMediator mediator
        // , IDepartmentGrpcClient departmentGrpcClient
        )
        {
            _assetTransferIssueQueryRepository = assetTransferIssueQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _departmentGrpcClient = departmentGrpcClient;
        }

        public async Task<ApiResponseDTO<List<AssetTransferIssueApprovalDto>>> Handle(GetAssetTranferIssueApprovalQuery request, CancellationToken cancellationToken)
        {
           var (assetIssueTransfer, totalCount) = await _assetTransferIssueQueryRepository
                                                .GetAllPendingAssetTransferAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var assetIssueTransferList = _mapper.Map<List<AssetTransferIssueApprovalDto>>(assetIssueTransfer);
            //    // 🔥 Fetch departments using gRPC
            // var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
            // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            //      var filteredassetIssueTransfer = assetIssueTransferList
            // .Where(p => departmentLookup.ContainsKey(p.FromDepartmentId))
            // .ToList();
          


            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",        
                actionName: assetIssueTransferList.Count.ToString(),
                details: $"Asset Transfer Pending details was fetched.",
                module:"Asset Transfer Pending"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetTransferIssueApprovalDto>>
            {
                IsSuccess = true,
                Message = "Success",
                // Data = filteredassetIssueTransfer,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize                
            };   
        }
    }
}