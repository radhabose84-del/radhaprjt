using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Domain.Entities.AssetMaster;
using MediatR;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ImportAssetAuditCommandHandler : IRequestHandler<ImportAssetAuditCommand, ApiResponseDTO<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService; 
        private readonly IExcelImportCommandRepository _assetRepository;

        public ImportAssetAuditCommandHandler(IMapper mapper, IIPAddressService ipAddressService, IExcelImportCommandRepository assetRepository, ITimeZoneService timeZoneService)
        {
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _assetRepository = assetRepository;
            _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ImportAssetAuditCommand request, CancellationToken cancellationToken)
        {
            if (request.ImportAssetAuditDto?.File == null || request.ImportAssetAuditDto.File.Length == 0)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "Invalid file uploaded",
                    Data = false
                };
            }

            var file = request.ImportAssetAuditDto.File;
            if (file.Length > 2 * 1024 * 1024)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "File size exceeds 2MB",
                    Data = false
                };
            }

            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId(); 
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 

            var assetAuditDtoList = new List<AssetAuditDto>();
            string uploadedFileName = file.FileName.Trim();

            if (await _assetRepository.CheckFileExistsAsync(uploadedFileName,  cancellationToken))
            
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"The file '{uploadedFileName}' has already been uploaded. Rename or upload a different file.",
                    Data = false
                };
           

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (!DateTime.TryParse(worksheet.Cells[row, 7].Text, out var auditDate))
                    {
                        return new ApiResponseDTO<bool>
                        {
                            IsSuccess = false,
                            Message = $"Invalid or missing AuditDate at row {row}. Please enter a valid date.",
                            Data = false
                        };
                    }
                    var dto = new AssetAuditDto
                    {
                        Sno = int.TryParse(worksheet.Cells[row, 1].Text, out int sno) ? sno : 0,
                        UnitName = worksheet.Cells[row, 2].Text?.Trim(),
                        AssetCode = worksheet.Cells[row, 3].Text?.Trim(),
                        AssetName = worksheet.Cells[row, 4].Text?.Trim(),
                        Department = worksheet.Cells[row, 5].Text?.Trim(),
                        AuditorName = worksheet.Cells[row, 6].Text?.Trim(),
                        AuditDate = auditDate,
                        AuditFinancialYear = GetFinancialYear(auditDate),
                        SourceFileName = file.FileName,
                        AuditTypeId = request.ImportAssetAuditDto.AuditCycle,
                        ScanType = "U",
                        Status = "Pending",
                        CreatedIP = currentIp,
                        CreatedDate = currentTime,
                        CreatedBy = userId,
                        CreatedByName = username
                    };

                    assetAuditDtoList.Add(dto);
                }

            }

            // ✅ Check for duplicate AssetCodes in Excel
            var duplicateAssetCodes = assetAuditDtoList
                .GroupBy(a => a.AssetCode?.Trim())
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key))
                .Select(g => new
                {
                    AssetCode = g.Key,
                    SNOs = string.Join(", ", g.Select(x => x.Sno))
                })
                .ToList();

            if (duplicateAssetCodes.Any())
            {
                var message = "Duplicate AssetCode(s) found in Excel:\n" +
                            string.Join("\n", duplicateAssetCodes.Select(d => $"AssetCode: {d.AssetCode}, SNO(s): {d.SNOs}"));

                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = message,
                    Data = false
                };
            }

            // Convert DTOs to entities here in the handler
            var entities = _mapper.Map<List<AssetAudit>>(assetAuditDtoList);

            var result = await _assetRepository.BulkInsertAsync(entities, cancellationToken);
            return new ApiResponseDTO<bool>
            {
                IsSuccess = result,
                Message = result ? "Audit assets imported successfully." : "Audit asset import failed.",
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