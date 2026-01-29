// using Contracts.Interfaces.External.IUser;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteDocumentAssetMasterGeneral
{
    public class DeleteDocumentAssetMasterGeneralCommandHandler : IRequestHandler<DeleteDocumentAssetMasterGeneralCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;        
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        private readonly ILogger<DeleteDocumentAssetMasterGeneralCommandHandler> _logger;
        private readonly IIPAddressService _ipAddressService;
        private readonly IAssetMasterGeneralCommandRepository _assetMasterGeneralRepository;              
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly ICompanyGrpcClient _companyGrpcClient;

        public DeleteDocumentAssetMasterGeneralCommandHandler(
            IFileUploadService fileUploadService,
            IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository,
            ILogger<DeleteDocumentAssetMasterGeneralCommandHandler> logger, IIPAddressService ipAddressService, IAssetMasterGeneralCommandRepository assetMasterGeneralRepository
            // , IUnitGrpcClient unitGrpcClient, ICompanyGrpcClient companyGrpcClient
            )
        {
            _fileUploadService = fileUploadService;            
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _logger = logger;  _ipAddressService = ipAddressService;_assetMasterGeneralRepository=assetMasterGeneralRepository;
            //  _unitGrpcClient = unitGrpcClient;
            // _companyGrpcClient = companyGrpcClient;            
        }

        public async Task<bool> Handle(DeleteDocumentAssetMasterGeneralCommand request, CancellationToken cancellationToken)
        { 
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            // //var (companyName, unitName) = await _assetMasterGeneralQueryRepository.GetCompanyUnitAsync(companyId, unitId);
            //  var companies = await _companyGrpcClient.GetAllCompanyAsync();
            // var units = await _unitGrpcClient.GetAllUnitAsync();

            // var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            // var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            // var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;
            
            string baseDirectory = await _assetMasterGeneralQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
                throw new Exception("Base directory not configured.");              
            }
            
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
            // , companyName, unitName
            );       

            string filePath = Path.Combine(uploadPath, request.assetPath??string.Empty);

            var result = await _fileUploadService.DeleteFileAsync(filePath);

            await _assetMasterGeneralRepository.RemoveAssetDocumentReferenceAsync(request.assetPath);

            if (result)
            {
                return result;
            }
            throw new Exception("File deletion failed");   
            
        }
    }
}
