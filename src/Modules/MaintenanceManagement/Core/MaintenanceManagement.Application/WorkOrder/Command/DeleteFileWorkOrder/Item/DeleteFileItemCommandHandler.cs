#nullable disable

using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder.Item
{
    public class DeleteFileItemCommandHandler : IRequestHandler<DeleteFileItemCommand, ApiResponseDTO<bool>>
    {
        private readonly IFileUploadService _fileUploadService;        
        private readonly IWorkOrderQueryRepository _woQueryRepository;        
        private readonly IIPAddressService _ipAddressService;
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public DeleteFileItemCommandHandler(
            IFileUploadService fileUploadService,            
            IWorkOrderQueryRepository woQueryRepository,
            ILogger<DeleteFileWorkOrderCommandHandler> logger, IIPAddressService ipAddressService,IWorkOrderCommandRepository workOrderRepository,IUnitLookup unitLookup, ICompanyLookup companyLookup)
        {
            _fileUploadService = fileUploadService;            
            _woQueryRepository = woQueryRepository;            
            _ipAddressService = ipAddressService;
            _workOrderRepository = workOrderRepository;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ApiResponseDTO<bool>> Handle(DeleteFileItemCommand request, CancellationToken cancellationToken)
        { 
            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;  

            string baseDirectory = await _workOrderRepository.GetBaseDirectoryItemAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {                
                return new ApiResponseDTO<bool> { IsSuccess = false, Message = "Base directory not configured." };                
            }
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);       

            string filePath = Path.Combine(uploadPath, request.Image??string.Empty);

            var result = await _fileUploadService.DeleteFileAsync(filePath);

            await _workOrderRepository.DeleteItemImageAsync( request.Image);

            if (result)
            {
                return new ApiResponseDTO<bool> { IsSuccess = true, Message = "File deleted successfully" };
            }            
            return new ApiResponseDTO<bool> { IsSuccess = false, Message = "File deletion failed" };
        }
    }
}
