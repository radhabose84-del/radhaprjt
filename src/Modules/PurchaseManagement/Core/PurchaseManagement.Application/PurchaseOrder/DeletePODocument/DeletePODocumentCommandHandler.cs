using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.DeletePODocument
{
    public class DeletePODocumentCommandHandler
        : IRequestHandler<DeletePODocumentCommand, ApiResponseDTO<bool>>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public DeletePODocumentCommandHandler(
            IFileUploadService fileUploadService,
            IIPAddressService ipAddressService,
            IPODocumentQueryRepository poDocumentQueryRepository,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _fileUploadService = fileUploadService;
            _ipAddressService = ipAddressService;
            _poDocumentQueryRepository = poDocumentQueryRepository;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<ApiResponseDTO<bool>> Handle(DeletePODocumentCommand request, CancellationToken cancellationToken)
        {
            // 1️⃣ Resolve company & unit names via gRPC
            var companyId = _ipAddressService.GetCompanyId();
            var unitId    = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var unit      = await _unitLookup.GetByIdAsync(unitId, cancellationToken);

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

            var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName    = unit?.UnitName ?? string.Empty;

            // 2️⃣ Base directory from DB/config
            var baseDirectory = await _poDocumentQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message   = "Base directory not configured.",
                    Data      = false
                };
            }

            // 3️⃣ Build physical path
            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                baseDirectory,
                companyName ?? string.Empty,
                unitName ?? string.Empty);

            var filePath = Path.Combine(uploadPath, request.PODocumentPath ?? string.Empty);

            if (!File.Exists(filePath))
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message   = "The specified file does not exist.",
                    Data      = false
                };
            }

            // 4️⃣ Delete physical file
            var result = await _fileUploadService.DeleteFileAsync(filePath);
            if (!result)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message   = "File deletion failed.",
                    Data      = false
                };
            }

            // 5️⃣ Delete DB metadata if IDs are provided
            if (request.Id > 0 && request.POId > 0)
            {
                var dbResult = await _poDocumentQueryRepository.DeleteFileDetailsDocumentAsync(
                    request.Id,
                    request.POId,
                    request.FileName ?? string.Empty);

                if (!dbResult)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message   = "Document entry not found or deletion failed in DB.",
                        Data      = false
                    };
                }
            }

            // 6️⃣ Success
            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message   = "PO document deleted successfully.",
                Data      = true
            };
        }
    }
}
