using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails
{
    public class GetAssetReceiptDetailsQueryHandler : IRequestHandler<GetAssetReceiptDetailsQuery,  ApiResponseDTO<List<AssetReceiptDetailsDto>>>
    {
        private readonly IAssetTransferReceiptQueryRepository _assetTransferReceiptQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;

        public GetAssetReceiptDetailsQueryHandler(IAssetTransferReceiptQueryRepository assetTransferReceiptQueryRepository, IMapper mapper, IMediator mediator,
            IUnitLookup unitLookup, IDepartmentLookup departmentLookup)
        {
            _assetTransferReceiptQueryRepository = assetTransferReceiptQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<AssetReceiptDetailsDto>>> Handle(GetAssetReceiptDetailsQuery request, CancellationToken cancellationToken)
        {
            var (AssetTransferReceipt, totalCount) = await _assetTransferReceiptQueryRepository
                                                .GetAllAssetReceiptDetails(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var assetreceiptlist = _mapper.Map<List<AssetReceiptDetailsDto>>(AssetTransferReceipt);

            // Enrich Unit names via lookup
            var unitIds = assetreceiptlist
                .SelectMany(x => new[] { x.FromUnitId, x.ToUnitId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (unitIds.Length > 0)
            {
                var units = await _unitLookup.GetByIdsAsync(unitIds, cancellationToken);
                var unitMap = units.Where(u => u != null).ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in assetreceiptlist)
                {
                    if (unitMap.TryGetValue(item.FromUnitId, out var fromUnitName))
                        item.FromUnitname = fromUnitName;
                    if (unitMap.TryGetValue(item.ToUnitId, out var toUnitName))
                        item.ToUnitname = toUnitName;
                }
            }

            // Enrich Department names via lookup
            var deptIds = assetreceiptlist
                .SelectMany(x => new[] { x.FromDepartmentId, x.ToDepartmentId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);
                var deptMap = departments.Where(d => d != null).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var item in assetreceiptlist)
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
                actionName: assetreceiptlist.Count.ToString(),
                details: $"Asset Receipt details was fetched.",
                module:"Asset Receipt"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetReceiptDetailsDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetreceiptlist,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}