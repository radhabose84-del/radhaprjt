using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval
{
    public class GetAssetTranferIssueApprovalQueryHandler : IRequestHandler<GetAssetTranferIssueApprovalQuery, ApiResponseDTO<List<AssetTransferIssueApprovalDto>>>
    {
        private readonly IAssetTransferIssueApprovalQueryRepository _assetTransferIssueQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;

        public GetAssetTranferIssueApprovalQueryHandler(IAssetTransferIssueApprovalQueryRepository assetTransferIssueQueryRepository, IMapper mapper, IMediator mediator,
            IUnitLookup unitLookup, IDepartmentLookup departmentLookup)
        {
            _assetTransferIssueQueryRepository = assetTransferIssueQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<AssetTransferIssueApprovalDto>>> Handle(GetAssetTranferIssueApprovalQuery request, CancellationToken cancellationToken)
        {
           var (assetIssueTransfer, totalCount) = await _assetTransferIssueQueryRepository
                                                .GetAllPendingAssetTransferAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var assetIssueTransferList = _mapper.Map<List<AssetTransferIssueApprovalDto>>(assetIssueTransfer);

            // Enrich Unit names via lookup
            var unitIds = assetIssueTransferList
                .SelectMany(x => new[] { x.FromUnitId, x.ToUnitId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (unitIds.Length > 0)
            {
                var units = await _unitLookup.GetByIdsAsync(unitIds, cancellationToken);
                var unitMap = units.Where(u => u != null).ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in assetIssueTransferList)
                {
                    if (unitMap.TryGetValue(item.FromUnitId, out var fromUnitName))
                        item.FromUnitname = fromUnitName;
                    if (unitMap.TryGetValue(item.ToUnitId, out var toUnitName))
                        item.ToUnitname = toUnitName;
                }
            }

            // Enrich Department names via lookup
            var deptIds = assetIssueTransferList
                .SelectMany(x => new[] { x.FromDepartmentId, x.ToDepartmentId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);
                var deptMap = departments.Where(d => d != null).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var item in assetIssueTransferList)
                {
                    if (deptMap.TryGetValue(item.FromDepartmentId, out var fromDeptName))
                        item.FromDepartment = fromDeptName;
                    if (deptMap.TryGetValue(item.ToDepartmentId, out var toDeptName))
                        item.ToDepartment = toDeptName;
                }
            }

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
                Data = assetIssueTransferList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };   
        }
    }
}