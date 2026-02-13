using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.UploadPODocument
{
    public class UploadPODocumentCommandHandler 
        : IRequestHandler<UploadPODocumentCommand, ApiResponseDTO<PODocumentDto>>
    {
        private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ILogger<UploadPODocumentCommandHandler> _logger;

        public UploadPODocumentCommandHandler(
            IPODocumentQueryRepository poDocumentQueryRepository,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup,
            ILogger<UploadPODocumentCommandHandler> logger)
        {
            _poDocumentQueryRepository = poDocumentQueryRepository;
            _ipAddressService          = ipAddressService;
            _unitLookup                = unitLookup;
            _companyLookup             = companyLookup;
            _logger                    = logger;
        }

        public async Task<ApiResponseDTO<PODocumentDto>> Handle(
            UploadPODocumentCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Basic validation
            if (request.File == null || request.File.Length == 0)
            {
                return new ApiResponseDTO<PODocumentDto>
                {
                    IsSuccess = false,
                    Message   = "No file uploaded.",
                    Data      = null
                };
            }

            // 2️⃣ Base directory from DB/config
            var baseDirectory = MiscEnumEntity.DocumentPath; //await _poDocumentQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
                return new ApiResponseDTO<PODocumentDto>
                {
                    IsSuccess = false,
                    Message   = "Base directory not configured.",
                    Data      = null
                };
            }

            // 3️⃣ Resolve company & unit names via gRPC
            var companyId = _ipAddressService.GetCompanyId();
            var unitId    = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var unit      = await _unitLookup.GetByIdAsync(unitId, cancellationToken);

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

            var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName    = unit?.UnitName ?? string.Empty;

            // 4️⃣ Build upload path
            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                baseDirectory,
                companyName,
                unitName);

            EnsureDirectoryExists(uploadPath);

            // 5️⃣ Build temp file name & path
            var fileExtension = Path.GetExtension(request.File.FileName);
            var dummyFileName = $"TEMP_{Guid.NewGuid()}{fileExtension}";
            var filePath      = Path.Combine(uploadPath, dummyFileName);

            try
            {
                // Ensure path exists
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));

                // Save file to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream, cancellationToken);
                }

                // Convert to Base64 (if needed for client)
                var fileBytes   = await File.ReadAllBytesAsync(filePath, cancellationToken);
                var base64Image = Convert.ToBase64String(fileBytes);

                var dto = new PODocumentDto
                {
                    FileName        = dummyFileName,  // only the relative file name
                    PODocumentBase64 = base64Image
                };

                return new ApiResponseDTO<PODocumentDto>
                {
                    IsSuccess = true,
                    Message   = "PO document uploaded successfully.",
                    Data      = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed for PO document.");

                return new ApiResponseDTO<PODocumentDto>
                {
                    IsSuccess = false,
                    Message   = $"File upload failed: {ex.Message}",
                    Data      = null
                };
            }
        }

        private static void EnsureDirectoryExists(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
