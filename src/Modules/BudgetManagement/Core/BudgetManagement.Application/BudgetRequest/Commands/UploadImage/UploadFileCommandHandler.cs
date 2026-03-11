using System.ComponentModel.DataAnnotations;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.BudgetRequest.Commands;
using BudgetManagement.Application.BudgetRequest.Commands.UploadImage;
using Contracts.Interfaces;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ImageDto>
    {
        private readonly ILogger<UploadFileCommandHandler> _logger;
        private readonly IBudgetRequestQueryRepository _budgetRequestQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public UploadFileCommandHandler(
            ILogger<UploadFileCommandHandler> logger,
            IBudgetRequestQueryRepository budgetRequestQueryRepository,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _logger = logger;
            _budgetRequestQueryRepository = budgetRequestQueryRepository;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ImageDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("No file uploaded");                
            }

             // 🔹 Fetch Base Directory from Database
            string baseDirectory = await _budgetRequestQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
                throw new ValidationException("Base directory not configured.");                
            }
  
            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyLookupDict.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitLookupDict.TryGetValue(unitId, out var uname) ? uname : string.Empty;   
            
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);                
            EnsureDirectoryExists(uploadPath);

            string fileExtension = Path.GetExtension(request.File.FileName);            
            string dummyFileName = $"TEMP_{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadPath, dummyFileName);

            try
            {
                EnsureDirectoryExists(Path.GetDirectoryName(filePath) ?? string.Empty);

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
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                
                throw new Exception($"File upload failed: {ex.Message}");
                
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
