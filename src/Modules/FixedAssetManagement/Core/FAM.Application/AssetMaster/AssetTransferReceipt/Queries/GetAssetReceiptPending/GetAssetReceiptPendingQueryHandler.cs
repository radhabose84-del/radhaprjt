using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending
{
    public class GetAssetReceiptPendingQueryHandler : IRequestHandler<GetAssetReceiptPendingQuery,  ApiResponseDTO<List<AssetTransferReceiptPendingDto>>>
    {
        private readonly IAssetTransferReceiptQueryRepository _assetTransferReceiptQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 
        // private readonly IDepartmentGrpcClient _departmentGrpcClient;

        public GetAssetReceiptPendingQueryHandler(IAssetTransferReceiptQueryRepository assetTransferReceiptQueryRepository, IMapper mapper, IMediator mediator
        // , IDepartmentGrpcClient departmentGrpcClient
        )
        {
            _assetTransferReceiptQueryRepository = assetTransferReceiptQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _departmentGrpcClient = departmentGrpcClient;
        }

        public async Task<ApiResponseDTO<List<AssetTransferReceiptPendingDto>>> Handle(GetAssetReceiptPendingQuery request, CancellationToken cancellationToken)
        {
             var (assetIssueTransfer, totalCount) = await _assetTransferReceiptQueryRepository
                                                .GetAllPendingAssetTransferAsync(request.PageNumber, request.PageSize,request.AssetTransferId ,request.SearchTerm, request.FromDate, request.ToDate);
            var assetIssueTransferList = _mapper.Map<List<AssetTransferReceiptPendingDto>>(assetIssueTransfer);
            //        // 🔥 Fetch departments using gRPC
            // var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
            // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            //      var filteredassetIssueTransfer = assetIssueTransferList
            // .Where(p => departmentLookup.ContainsKey(p.ToDepartmentId))
            // .ToList();

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",        
                actionName: assetIssueTransferList.Count.ToString(),
                details: $"Asset Receipt Pending details was fetched.",
                module:"Asset Receipt Pending"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetTransferReceiptPendingDto>>
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