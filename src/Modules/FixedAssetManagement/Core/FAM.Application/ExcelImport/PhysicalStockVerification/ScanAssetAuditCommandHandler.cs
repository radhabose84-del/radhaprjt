#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Domain.Entities.AssetMaster;
using MediatR;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ScanAssetAuditCommandHandler : IRequestHandler<ScanAssetAuditCommand, ApiResponseDTO<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IExcelImportCommandRepository _assetRepository;

        public ScanAssetAuditCommandHandler(IMapper mapper, IIPAddressService ipAddressService, IExcelImportCommandRepository assetRepository, ITimeZoneService timeZoneService)
        {
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _assetRepository = assetRepository;
            _timeZoneService = timeZoneService;
        }

    public async Task<ApiResponseDTO<bool>> Handle(ScanAssetAuditCommand request, CancellationToken cancellationToken)
       {
        string currentIp = _ipAddressService.GetSystemIPAddress();
        int userId = _ipAddressService.GetUserId();
        string username = _ipAddressService.GetUserName();
        var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
        var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); // DateTimeOffset
        var auditDate = new DateTimeOffset(currentTime.Date, currentTime.Offset);
         // ✅ Check for duplicate entry
        var alreadyScanned = await _assetRepository
            .IsAssetAlreadyScannedAsync(request.AssetCode, request.AuditCycle,GetFinancialYear(currentTime.DateTime),request.DepartmentName,request.UnitName ,cancellationToken);

        if (alreadyScanned)
        {
            return new ApiResponseDTO<bool>
            {
                IsSuccess = false,
                Message = $"AssetCode- '{request.AssetCode}' is already scanned for this audit cycle.",
                Data = false
            };
        }

        var entity = new AssetAudit
        {
            AssetCode = request.AssetCode,
            AuditDate = auditDate,  
            AuditorName=username,                 // ✅ Only one AuditDate
            CreatedDate = currentTime,
            CreatedByName = username,
            CreatedIP = currentIp,
            AuditTypeId = request.AuditCycle,
            CreatedBy = userId,
            ScanType = "S",
            Status = "Pending",
            UnitName=request.UnitName,
            Department =request.DepartmentName,
            AuditFinancialYear = GetFinancialYear(currentTime.DateTime) // ✅ Convert to DateTime
        };

        var result = await _assetRepository.InsertScannedAssetAsync(entity, cancellationToken);
        return new ApiResponseDTO<bool>
        {
            IsSuccess = result,
            Message = result ? "Scanned asset inserted successfully." : "Failed to insert scanned asset.",
            Data = result
        };
    }
        private string GetFinancialYear(DateTime date)
        {
            return date.Month >= 4
                ? $"{date.Year}-{date.Year + 1}"
                : $"{date.Year - 1}-{date.Year}";
        }
    }
}