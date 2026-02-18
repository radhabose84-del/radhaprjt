using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending
{
    public class GetAssetRecieptDtlPendingQueryHandler : IRequestHandler<GetAssetRecieptDtlPendingQuery, AssetTrasnferReceiptHdrPendingDto>
    {

        private readonly IAssetTransferReceiptQueryRepository _assetTransferReceiptQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;

        public GetAssetRecieptDtlPendingQueryHandler(IAssetTransferReceiptQueryRepository assetTransferReceiptQueryRepository, IMapper mapper, IMediator mediator,
            IUnitLookup unitLookup, IDepartmentLookup departmentLookup)
        {
            _assetTransferReceiptQueryRepository = assetTransferReceiptQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
        }

        public async Task<AssetTrasnferReceiptHdrPendingDto> Handle(GetAssetRecieptDtlPendingQuery request, CancellationToken cancellationToken)
        {
             var assetTransfer = await _assetTransferReceiptQueryRepository.GetAssetTransferByIdAsync(request.AssetTransferId);

            if (assetTransfer == null)
            {
                throw new ValidationException($"Asset Transfer Issue with ID {request.AssetTransferId} not found.");
            }

            // Enrich Unit names via lookup
            var unitIds = new[] { assetTransfer.FromUnitId, assetTransfer.ToUnitId }
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (unitIds.Length > 0)
            {
                var units = await _unitLookup.GetByIdsAsync(unitIds, cancellationToken);
                var unitMap = units.Where(u => u != null).ToDictionary(u => u.UnitId, u => u.UnitName);

                if (unitMap.TryGetValue(assetTransfer.FromUnitId, out var fromUnitName))
                    assetTransfer.FromUnitname = fromUnitName;
                if (unitMap.TryGetValue(assetTransfer.ToUnitId, out var toUnitName))
                    assetTransfer.ToUnitname = toUnitName;
            }

            // Enrich Department names via lookup
            var deptIds = new[] { assetTransfer.FromDepartmentId, assetTransfer.ToDepartmentId }
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);
                var deptMap = departments.Where(d => d != null).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                if (deptMap.TryGetValue(assetTransfer.FromDepartmentId, out var fromDeptName))
                    assetTransfer.FromDepartment = fromDeptName;
                if (deptMap.TryGetValue(assetTransfer.ToDepartmentId, out var toDeptName))
                    assetTransfer.ToDepartment = toDeptName;
            }

            return assetTransfer;
        }
    }
}