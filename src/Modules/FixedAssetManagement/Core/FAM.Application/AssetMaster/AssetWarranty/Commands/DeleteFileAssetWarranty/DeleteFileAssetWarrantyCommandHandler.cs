using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteFileAssetWarranty
{
    public class DeleteFileAssetWarrantyCommandHandler : IRequestHandler<DeleteFileAssetWarrantyCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IAssetWarrantyCommandRepository _assetWarrantyRepository;
        private readonly ILogger<DeleteFileAssetWarrantyCommandHandler> _logger;
        private readonly IIPAddressService _ipAddressService;
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralRepository;
        private readonly IAssetWarrantyQueryRepository _assetWarrantQueryRepository;
        private readonly ICompanyLookup _companyLookup;  // ✅ lookup dependency
        private readonly IUnitLookup _unitLookup;        // ✅ lookup dependency

        public DeleteFileAssetWarrantyCommandHandler(
            IFileUploadService fileUploadService,
            IAssetWarrantyCommandRepository assetWarrantyRepository,
            ILogger<DeleteFileAssetWarrantyCommandHandler> logger, IIPAddressService ipAddressService, IAssetMasterGeneralQueryRepository assetMasterGeneralRepository, IAssetWarrantyQueryRepository assetWarrantQueryRepository,
            ICompanyLookup companyLookup,  // ✅ inject lookup
            IUnitLookup unitLookup)        // ✅ inject lookup
        {
            _fileUploadService = fileUploadService;
            _assetWarrantyRepository = assetWarrantyRepository;
            _logger = logger; _ipAddressService = ipAddressService;_assetMasterGeneralRepository=assetMasterGeneralRepository;_assetWarrantQueryRepository=assetWarrantQueryRepository;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
        }

       public async Task<bool> Handle(DeleteFileAssetWarrantyCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // ✅ Get company and unit names using lookup interfaces
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();
            var companyMap = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitMap = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyMap.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitMap.TryGetValue(unitId, out var uname) ? uname : string.Empty;

            string baseDirectory = await _assetWarrantQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
                throw new ValidationException("Base directory not configured.");
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName, unitName);       

            string filePath = Path.Combine(uploadPath, request.assetPath??string.Empty);

            var result = await _fileUploadService.DeleteFileAsync(filePath);

            await _assetWarrantyRepository.RemoveAssetWarrantyAsync(request.assetPath ?? string.Empty);
              if (result)
            {
                return result;
            }
            throw new Exception("File deletion failed");
            
        }

    }
}
