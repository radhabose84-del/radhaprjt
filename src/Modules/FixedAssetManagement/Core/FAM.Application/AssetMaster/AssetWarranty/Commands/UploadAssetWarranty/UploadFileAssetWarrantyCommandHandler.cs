using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty
{
    public class UploadFileAssetWarrantyCommandHandler : IRequestHandler<UploadFileAssetWarrantyCommand, AssetWarrantyDTO>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAssetWarrantyCommandRepository _assetWarrantyRepository;
        private readonly IAssetWarrantyQueryRepository _assetWarrantyQueryRepository;
        private readonly ILogger<UploadFileAssetWarrantyCommandHandler> _logger;
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;  // ✅ lookup dependency
        private readonly IUnitLookup _unitLookup;        // ✅ lookup dependency

        public UploadFileAssetWarrantyCommandHandler(
            IFileUploadService fileUploadService,
            IMediator mediator,
            IMapper mapper,
            IAssetWarrantyCommandRepository assetWarrantyRepository,
            ILogger<UploadFileAssetWarrantyCommandHandler> logger, IAssetWarrantyQueryRepository assetWarrantyQueryRepository, IIPAddressService ipAddressService, IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository,
            ICompanyLookup companyLookup,  // ✅ inject lookup
            IUnitLookup unitLookup)        // ✅ inject lookup
        {
            _fileUploadService = fileUploadService;
            _mediator = mediator;
            _mapper = mapper;
            _assetWarrantyRepository = assetWarrantyRepository;
            _logger = logger;
            _assetWarrantyQueryRepository=assetWarrantyQueryRepository;
            _ipAddressService = ipAddressService;
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
        }

        public async Task<AssetWarrantyDTO> Handle(UploadFileAssetWarrantyCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("No file uploaded");
                
            }

            if (string.IsNullOrWhiteSpace(request.AssetCode))
            {
                throw new ValidationException("AssetCode is required for file naming.");
                
            }

            // 🔹 Check if asset exists using repository
            var existingAsset = await _assetWarrantyRepository.GetByAssetCodeAsync(request.AssetCode);
            if (existingAsset == null)
            {
                throw new ValidationException("Asset not found.");
                
            }

            try
            {
                // 🔹 Define Base Directory
                string baseDirectory = await _assetWarrantyQueryRepository.GetBaseDirectoryAsync();                
                EnsureDirectoryExists(baseDirectory);

                var companyId = _ipAddressService.GetCompanyId();
                var unitId = _ipAddressService.GetUnitId();

                // ✅ Get company and unit names using lookup interfaces
                var companies = await _companyLookup.GetAllCompanyAsync();
                var units = await _unitLookup.GetAllUnitAsync();
                var companyMap = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                var unitMap = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var companyName = companyMap.TryGetValue(companyId, out var cname) ? cname : string.Empty;
                var unitName = unitMap.TryGetValue(unitId, out var uname) ? uname : string.Empty;

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName, unitName);                
                EnsureDirectoryExists(uploadPath);

                string fileExtension = Path.GetExtension(request.File.FileName);
                string fileName = $"{"WARRANTY_"}{request.AssetCode}{fileExtension}"; // ✅ Example: HomeTextile-COMP-MOU-1.png
                string filePath = Path.Combine(uploadPath, fileName);

                EnsureDirectoryExists(Path.GetDirectoryName(filePath));

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                // Convert Image to Base64 (optional)
                string base64Image = Convert.ToBase64String(await File.ReadAllBytesAsync(filePath));

                // ✅ Ensure correct format before saving in DB
                string formattedPath = filePath.Replace(@"\", "/");

                // ✅ Update AssetImage field using repository
                bool updateSuccess = await _assetWarrantyRepository.UpdateAssetWarrantyImageAsync(existingAsset.Id, formattedPath);
                if (!updateSuccess)
                {
                    throw new ValidationException("Failed to update asset image.");
                    
                }

                var response = new AssetWarrantyDTO
                {
                    Document = formattedPath,  // ✅ Correctly formatted file path
                    DocumentBase64 = base64Image  // ✅ Convert to Base64
                };

                return  response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                throw new Exception($"File upload failed: {ex.Message}");
                
            }
        }

        // ✅ Helper Method to Ensure Directory Exists
        private void EnsureDirectoryExists(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
