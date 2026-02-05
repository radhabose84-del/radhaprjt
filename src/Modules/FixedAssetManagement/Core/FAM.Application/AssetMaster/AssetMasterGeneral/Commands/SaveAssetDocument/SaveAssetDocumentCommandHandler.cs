using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.SaveAssetDocument
{
    public class SaveAssetDocumentCommandHandler : IRequestHandler<SaveAssetDocumentCommand, bool>
    {
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        private readonly IAssetMasterGeneralCommandRepository _assetMasterGeneralRepository;
        private readonly ILogger<UploadDocumentAssetMasterGeneralCommandHandler> _logger;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;  // ✅ lookup dependency
        private readonly IUnitLookup _unitLookup;        // ✅ lookup dependency

        public SaveAssetDocumentCommandHandler(
            IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository,
            ILogger<UploadDocumentAssetMasterGeneralCommandHandler> logger, IIPAddressService ipAddressService, IAssetMasterGeneralCommandRepository assetMasterGeneralRepository,
            ICompanyLookup companyLookup,  // ✅ inject lookup
            IUnitLookup unitLookup)        // ✅ inject lookup
        {
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _logger = logger;
            _ipAddressService = ipAddressService;
            _assetMasterGeneralRepository = assetMasterGeneralRepository;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
        }

        public async Task<bool> Handle(SaveAssetDocumentCommand request, CancellationToken cancellationToken)
        {
             if (request.assetPath == null || request.assetPath.Length == 0)
            {
                throw new ValidationException("No file uploaded");
                
            }

            string tempFilePath = request.assetPath;
            if (tempFilePath != null){
                string baseDirectory = await _assetMasterGeneralQueryRepository.GetDocumentDirectoryAsync();
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

                string filePath = Path.Combine(uploadPath, tempFilePath);  
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));           

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                    string newFileName = $"{request.AssetCode}{Path.GetExtension(tempFilePath)}";
                    string newFilePath = Path.Combine(directory, newFileName);

                    try
                    {
                        File.Move(filePath, newFilePath);
                        //assetEntity.AssetImage = newFileName;
                        await _assetMasterGeneralRepository.UpdateDocumentAsync(request.Id, newFileName);
                    }
                    catch (Exception ex)
                    {
                        Log.Information(ex, "Failed to rename file.");
                    }
                }
            }                    
            return true;
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