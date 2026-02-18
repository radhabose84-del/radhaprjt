#nullable disable


using System;
using System.IO;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder
{
    public class UploadFileWorkOrderCommandHandler : IRequestHandler<UploadFileWorkOrderCommand, ApiResponseDTO<WorkOrderImageDto>>
    {  
        private readonly IWorkOrderQueryRepository _woQueryRepository;
        private readonly ILogger<UploadFileWorkOrderCommandHandler> _logger;
         private readonly IIPAddressService _ipAddressService;
         private readonly IWorkOrderCommandRepository _workOrderRepository;
         private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public UploadFileWorkOrderCommandHandler(
            IWorkOrderQueryRepository woQueryRepository,
            ILogger<UploadFileWorkOrderCommandHandler> logger, IIPAddressService ipAddressService, IWorkOrderCommandRepository workOrderRepository,
            IUnitLookup unitLookup, ICompanyLookup companyLookup)
        {           
            _woQueryRepository = woQueryRepository;
            _logger = logger;
            _ipAddressService = ipAddressService;
            _workOrderRepository = workOrderRepository;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ApiResponseDTO<WorkOrderImageDto>> Handle(UploadFileWorkOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new ApiResponseDTO<WorkOrderImageDto> { IsSuccess = false, Message = "No file uploaded" };
            }           
             // 🔹 Fetch Base Directory from Database
            string baseDirectory = await _woQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
               _logger.LogError("Base directory path not found in database.");
                return new ApiResponseDTO<WorkOrderImageDto> { IsSuccess = false, Message = "Base directory not configured." };
            }
            var companyId =_ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();
    
            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;   

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
            // , companyName, unitName
            );                
            EnsureDirectoryExists(uploadPath);

            string fileExtension = Path.GetExtension(request.File.FileName);            
            string dummyFileName = $"TEMP_{Guid.NewGuid()}{fileExtension}";            
            string filePath = Path.Combine(uploadPath, dummyFileName);

            try
            {
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                // Convert Image to Base64 (optional)
                string base64Image = Convert.ToBase64String(await File.ReadAllBytesAsync(filePath));

                // ✅ Ensure the correct format before saving in DB
                string formattedPath = dummyFileName;
                 var response = new WorkOrderImageDto
                {
                    WorkOrderImage = formattedPath,  // ✅ Correctly formatted file path
                    WorkOrderImageBase64 = base64Image  // ✅ Convert to Base64
                };

                return new ApiResponseDTO<WorkOrderImageDto> { IsSuccess = true, Data = response };
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                return new ApiResponseDTO<WorkOrderImageDto> { IsSuccess = false, Message = $"File upload failed: {ex.Message}" };
            }
        }   
        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}
