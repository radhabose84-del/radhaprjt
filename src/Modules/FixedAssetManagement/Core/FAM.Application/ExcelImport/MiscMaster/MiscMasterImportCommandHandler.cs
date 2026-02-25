#nullable disable
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IMiscMaster;
using MediatR;
using OfficeOpenXml;
using static FAM.Domain.Common.BaseEntity;


namespace FAM.Application.ExcelImport.MiscMaster
{
    public class MiscMasterImportCommandHandler : IRequestHandler<MiscMasterImportCommand, ApiResponseDTO<bool>>
    {

        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;

        public MiscMasterImportCommandHandler(IMapper mapper, IIPAddressService ipAddressService, ITimeZoneService timeZoneService, IMiscMasterQueryRepository miscMasterQueryRepository, IMiscMasterCommandRepository miscMasterCommandRepository)
        {
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _miscMasterCommandRepository = miscMasterCommandRepository;
        }

        // public async Task<ApiResponseDTO<bool>> Handle(MiscMasterImportCommand request, CancellationToken cancellationToken)
        // {
        //     throw new NotImplementedException();
        // }
        public async Task<ApiResponseDTO<bool>> Handle(MiscMasterImportCommand request, CancellationToken cancellationToken)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return new ApiResponseDTO<bool> { IsSuccess = false, Message = "Invalid file uploaded", Data = false };

            if (file.Length > 2 * 1024 * 1024)
                return new ApiResponseDTO<bool> { IsSuccess = false, Message = "File size exceeds 2MB", Data = false };

            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            var miscMasterDtoList = new List<MiscMasterImportDto>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // skip header row
                {
                    var idParsed = int.TryParse(worksheet.Cells[row, 1].Text, out int id);
                    var miscTypeIdParsed = int.TryParse(worksheet.Cells[row, 2].Text, out int miscTypeId);
                    var sortOrderParsed = int.TryParse(worksheet.Cells[row, 5].Text, out int sortOrder);
                    var isActiveParsed = int.TryParse(worksheet.Cells[row, 6].Text, out int isActive);
                    var isDeletedParsed = int.TryParse(worksheet.Cells[row, 7].Text, out int isDeleted);
                    var createdByParsed = int.TryParse(worksheet.Cells[row, 8].Text, out int createdBy);
                    var createdDateParsed = DateTimeOffset.TryParse(worksheet.Cells[row, 9].Text, out DateTimeOffset createdDate);
                    var modifiedByParsed = int.TryParse(worksheet.Cells[row, 12].Text, out int modifiedBy);
                    var modifiedDateParsed = DateTimeOffset.TryParse(worksheet.Cells[row, 13].Text, out DateTimeOffset modifiedDate);

                    var dto = new MiscMasterImportDto
                    {
                        Id = idParsed ? id : 0,
                        MiscTypeId = miscTypeIdParsed ? miscTypeId : 0,
                        Code = worksheet.Cells[row, 3].Text?.Trim(),
                        Description = worksheet.Cells[row, 4].Text?.Trim(),
                        SortOrder = sortOrderParsed ? sortOrder : 0,
                        IsActive = isActiveParsed ? (Status)isActive : Status.Inactive,
                        IsDeleted = isDeletedParsed ? (IsDelete)isDeleted : IsDelete.NotDeleted,
                        CreatedBy = createdByParsed ? createdBy : 0,
                        CreatedDate = createdDateParsed ? createdDate : DateTimeOffset.Now,
                        CreatedByName = worksheet.Cells[row, 10].Text?.Trim(),
                        CreatedIP = worksheet.Cells[row, 11].Text?.Trim() ?? currentIp,
                        ModifiedBy = modifiedByParsed ? modifiedBy : (int?)null,
                        ModifiedDate = modifiedDateParsed ? modifiedDate : (DateTimeOffset?)null,
                        ModifiedByName = worksheet.Cells[row, 14].Text?.Trim(),
                        ModifiedIP = worksheet.Cells[row, 15].Text?.Trim()
                    };

                    miscMasterDtoList.Add(dto);
                }
            }

            // Validate duplicates within the Excel file
            var duplicates = miscMasterDtoList
                .GroupBy(x => new { x.MiscTypeId, x.Code })
                .Where(g => g.Count() > 1)
                .Select(g => new { g.Key.MiscTypeId, g.Key.Code })
                .ToList();

            if (duplicates.Any())
            {
                var dupMessages = string.Join(", ", duplicates.Select(d => $"(MiscTypeId: {d.MiscTypeId}, Code: '{d.Code}')"));
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"Duplicate MiscTypeId + Code combinations found in Excel: {dupMessages}",
                    Data = false
                };
            }

            // Save or update
            foreach (var misc in miscMasterDtoList)
            {
                var existing = await _miscMasterQueryRepository.GetByMiscTypeIdAndCodeAsync(misc.MiscTypeId, misc.Code);

               
                  if (existing == null)
                    {
                        // Insert new
                        var entity = new FAM.Domain.Entities.MiscMaster
                        {
                            MiscTypeId = misc.MiscTypeId,
                            Code = misc.Code,
                            Description = misc.Description,
                            SortOrder = misc.SortOrder,
                            IsActive = (Status)misc.IsActive,
                            IsDeleted = (IsDelete)misc.IsDeleted,
                            CreatedBy = misc.CreatedBy,
                            CreatedDate = misc.CreatedDate.UtcDateTime,
                            CreatedByName = misc.CreatedByName,
                            CreatedIP = misc.CreatedIP,
                            ModifiedBy = misc.ModifiedBy,
                            ModifiedDate = misc.ModifiedDate?.UtcDateTime,
                            ModifiedByName = misc.ModifiedByName,
                            ModifiedIP = misc.ModifiedIP
                        };

                        await _miscMasterCommandRepository.AddAsync(entity);
                    }
                    else
                    {
                        // Update existing
                        existing.Description = misc.Description;
                        existing.SortOrder = misc.SortOrder;
                        existing.IsActive = (Status)misc.IsActive;
                        existing.IsDeleted = (IsDelete)misc.IsDeleted;
                        existing.ModifiedBy = misc.ModifiedBy;
                        existing.ModifiedDate = misc.ModifiedDate?.UtcDateTime;
                        existing.ModifiedByName = misc.ModifiedByName;
                        existing.ModifiedIP = misc.ModifiedIP;

                        await _miscMasterCommandRepository.UpdateMiscUploadAsync(existing);
                    }
            }

            await _miscMasterCommandRepository.SaveChangesAsync();

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "MiscMaster data imported successfully.",
                Data = true
            };
            
            
        }

    
    }
}