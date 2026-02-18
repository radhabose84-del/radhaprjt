using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransfer.Queries.GetAssetTransfered
{
    public class AssetTransferQueryHandler : IRequestHandler<AssetTransferQuery,  ApiResponseDTO<List<AssetTransferDto>>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;

        public AssetTransferQueryHandler( IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator,
            IUnitLookup unitLookup, IDepartmentLookup departmentLookup)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
             _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
        }
         public  async Task<ApiResponseDTO<List<AssetTransferDto>>> Handle(AssetTransferQuery request, CancellationToken cancellationToken)
        {
            var (assetTransferList, totalCount)  = await _assetTransferQueryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm ,request.FromDate, request.ToDate);
            var AssetTransferList = _mapper.Map<List<AssetTransferDto>>(assetTransferList);

            // Enrich Unit names via lookup
            var unitIds = AssetTransferList
                .SelectMany(x => new[] { x.FromUnitId, x.ToUnitId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (unitIds.Length > 0)
            {
                var units = await _unitLookup.GetByIdsAsync(unitIds, cancellationToken);
                var unitMap = units.Where(u => u != null).ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in AssetTransferList)
                {
                    if (unitMap.TryGetValue(item.FromUnitId, out var fromUnitName))
                        item.FromUnitName = fromUnitName;
                    if (unitMap.TryGetValue(item.ToUnitId, out var toUnitName))
                        item.ToUnitName = toUnitName;
                }
            }

            // Enrich Department names via lookup
            var deptIds = AssetTransferList
                .SelectMany(x => new[] { x.FromDepartmentId, x.ToDepartmentId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);
                var deptMap = departments.Where(d => d != null).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var item in AssetTransferList)
                {
                    if (deptMap.TryGetValue(item.FromDepartmentId, out var fromDeptName))
                        item.FromDepartmentName = fromDeptName;
                    if (deptMap.TryGetValue(item.ToDepartmentId, out var toDeptName))
                        item.ToDepartmentName = toDeptName;
                }
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: $"Asset Transfer    details was fetched.",
                module:"Asset Insurance"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetTransferDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = AssetTransferList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

      
        
    }
}