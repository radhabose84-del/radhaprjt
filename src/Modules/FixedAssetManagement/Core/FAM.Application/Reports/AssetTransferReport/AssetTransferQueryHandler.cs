using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using Contracts.Common;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.Reports.AssetTransferReport
{
    public class AssetTransferQueryHandler : IRequestHandler<AssetTransferQuery, ApiResponseDTO<List<AssetTransferDetailsDto>>>
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;  // ✅ lookup dependency

        public AssetTransferQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator,
            IDepartmentLookup departmentLookup) // ✅ inject lookup
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<AssetTransferDetailsDto>>> Handle(AssetTransferQuery request, CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? throw new ArgumentNullException(nameof(request.FromDate));
            var toDate = request.ToDate ?? throw new ArgumentNullException(nameof(request.ToDate));

            // Fetch AssetTransfer report data from repository
            var assetTransfersReports = await _repository.AssetTransferReportAsync(fromDate, toDate);

            // Map to DTOs
            var assetTransfersReportDtos = _mapper.Map<List<AssetTransferDetailsDto>>(assetTransfersReports);

            // ✅ Enrich DepartmentName using lookup interface (UserManagement owner)
            var deptIds = assetTransfersReportDtos
                .SelectMany(x => new[] { x.FromDepartmentId, x.ToDepartmentId })
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);

                var deptMap = departments
                    .Where(d => d != null)
                    .ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var item in assetTransfersReportDtos)
                {
                    if (deptMap.TryGetValue(item.FromDepartmentId, out var fromDeptName))
                    {
                        item.FromDepartmentName = fromDeptName;
                    }
                    if (deptMap.TryGetValue(item.ToDepartmentId, out var toDeptName))
                    {
                        item.ToDepartmentName = toDeptName;
                    }
                }
            }

            // Log audit
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAssetTransferReport",
                actionCode: "Get",
                actionName: assetTransfersReportDtos.Count.ToString(),
                details: "AssetTransfer report list fetched.",
                module: "AssetTransfer Reports"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // Return API response
            return new ApiResponseDTO<List<AssetTransferDetailsDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetTransfersReportDtos ?? new List<AssetTransferDetailsDto>(),
                TotalCount = assetTransfersReportDtos?.Count ?? 0
            };
        }
    }
}
