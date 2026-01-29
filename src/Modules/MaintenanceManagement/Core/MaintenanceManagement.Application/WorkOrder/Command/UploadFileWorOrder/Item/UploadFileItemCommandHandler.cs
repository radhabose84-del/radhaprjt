
using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item
{
    public class UploadFileItemCommandHandler : IRequestHandler<UploadFileItemCommand, ApiResponseDTO<ItemImageDto>>
    {
        
        private readonly ILogger<UploadFileWorkOrderCommandHandler> _logger;
         private readonly IIPAddressService _ipAddressService;
         private readonly IWorkOrderCommandRepository _workOrderRepository;
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly ICompanyGrpcClient _companyGrpcClient;

        public UploadFileItemCommandHandler(
            ILogger<UploadFileWorkOrderCommandHandler> logger, IIPAddressService ipAddressService, IWorkOrderCommandRepository workOrderRepository
            // , IUnitGrpcClient unitGrpcClient, ICompanyGrpcClient companyGrpcClient
            )
        {               
            _logger = logger;
            _ipAddressService = ipAddressService;
            _workOrderRepository = workOrderRepository;
            // _unitGrpcClient = unitGrpcClient;
            // _companyGrpcClient = companyGrpcClient;
        }

        public async Task<ApiResponseDTO<ItemImageDto>> Handle(UploadFileItemCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new ApiResponseDTO<ItemImageDto> { IsSuccess = false, Message = "No file uploaded" };
            }           
             // 🔹 Fetch Base Directory from Database
            string baseDirectory = await _workOrderRepository.GetBaseDirectoryItemAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
               _logger.LogError("Base directory path not found in database.");
                return new ApiResponseDTO<ItemImageDto> { IsSuccess = false, Message = "Base directory not configured." };
            }
            var companyId =_ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            //  var companies = await _companyGrpcClient.GetAllCompanyAsync();
            // var units = await _unitGrpcClient.GetAllUnitAsync();

            // var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            // var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            // var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;   

            try
            {                
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
                // , companyName, unitName
                );        
                EnsureDirectoryExists(uploadPath);                

                string fileExtension = Path.GetExtension(request.File.FileName);            
                string dummyFileName = $"TEMP_{Guid.NewGuid()}{fileExtension}";            
                string filePath = Path.Combine(uploadPath, dummyFileName);
                
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
                 var response = new ItemImageDto
                {
                    WorkOrderItemImage = formattedPath,  // ✅ Correctly formatted file path
                    WorkOrderImageItemBase64 = base64Image  // ✅ Convert to Base64
                };

                return new ApiResponseDTO<ItemImageDto> { IsSuccess = true, Data = response };
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                return new ApiResponseDTO<ItemImageDto> { IsSuccess = false, Message = $"File upload failed: {ex.Message}" };
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
