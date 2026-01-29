

// using Contracts.Interfaces.External.IUser;
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
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly ICompanyGrpcClient _companyGrpcClient;

          public SaveAssetDocumentCommandHandler(
            IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository,
            ILogger<UploadDocumentAssetMasterGeneralCommandHandler> logger, IIPAddressService ipAddressService, IAssetMasterGeneralCommandRepository assetMasterGeneralRepository
            // , IUnitGrpcClient unitGrpcClient, ICompanyGrpcClient companyGrpcClient
            )
        {          
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _logger = logger;
            _ipAddressService = ipAddressService;
            _assetMasterGeneralRepository = assetMasterGeneralRepository;
            // _unitGrpcClient = unitGrpcClient;
            // _companyGrpcClient = companyGrpcClient;
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
                // var companyId =_ipAddressService.GetCompanyId();
                // var unitId = _ipAddressService.GetUnitId();
                
                // var companies = await _companyGrpcClient.GetAllCompanyAsync();
                // var units = await _unitGrpcClient.GetAllUnitAsync();

                // var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                // var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
                // var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;

                //var (companyName, unitName) = await _assetMasterGeneralQueryRepository.GetCompanyUnitAsync(companyId, unitId);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
                // , companyName, unitName
                );    

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