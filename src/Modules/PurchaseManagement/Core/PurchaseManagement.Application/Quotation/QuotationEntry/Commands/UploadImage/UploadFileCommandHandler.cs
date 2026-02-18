#nullable disable

using System.ComponentModel.DataAnnotations;
using Contracts.Interfaces.External.IUser;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.Item.ItemDetail.Commands.UploadItemImage
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ApiResponseDTO<ImageDto>>
    {
        private readonly ILogger<UploadFileCommandHandler> _logger;                
        private readonly IQuotationCommandRepository _quotationCommandRepository;
        private readonly IIPAddressService _ipAddressService;        
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public UploadFileCommandHandler(
            ILogger<UploadFileCommandHandler> logger,
             IQuotationCommandRepository quotationCommandRepository, IIPAddressService ipAddressService, IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _logger = logger;
            _quotationCommandRepository = quotationCommandRepository;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ApiResponseDTO<ImageDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new ApiResponseDTO<ImageDto> { IsSuccess = false, Message = "No file uploaded" };
            }           

               string baseDirectory = await _quotationCommandRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
               _logger.LogError("Base directory path not found in database.");
                return new ApiResponseDTO<ImageDto> { IsSuccess = false, Message = "Base directory not configured." };
            }
            var companyId =_ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            
               var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;   

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);                
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
                var response = new ImageDto
                {
                    Image = formattedPath,
                    ImageBase64 = base64Image
                };

                return new ApiResponseDTO<ImageDto> { IsSuccess = true, Data = response };
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                return new ApiResponseDTO<ImageDto> { IsSuccess = false, Message = $"File upload failed: {ex.Message}" };
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
