using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using Serilog;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral
{
    public class UpdateAssetMasterGeneralCommandHandler : IRequestHandler<UpdateAssetMasterGeneralCommand, bool>
    {
        private readonly IAssetMasterGeneralCommandRepository _assetMasterGeneralRepository;
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICompanyLookup _companyLookup;  // ✅ lookup dependency
        private readonly IUnitLookup _unitLookup;        // ✅ lookup dependency

        public UpdateAssetMasterGeneralCommandHandler(IAssetMasterGeneralCommandRepository assetMasterGeneralRepository, IMapper mapper, IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository, IMediator mediator,
            ICompanyLookup companyLookup,  // ✅ inject lookup
            IUnitLookup unitLookup)        // ✅ inject lookup
        {
            _assetMasterGeneralRepository = assetMasterGeneralRepository;
            _mapper = mapper;
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _mediator = mediator;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
        }

        public async Task<bool> Handle(UpdateAssetMasterGeneralCommand request, CancellationToken cancellationToken)
        {
            var assetMaster = await _assetMasterGeneralQueryRepository.GetByIdAsync(request.AssetMaster.Id);
            if (assetMaster is null)
            throw new ValidationException("Invalid AssetId. The specified AssetName does not exist or is inactive.");
          
            var oldAssetName = assetMaster.AssetName;
            assetMaster.AssetName = request.AssetMaster.AssetName;

         
            var updatedAssetMasterEntity = _mapper.Map<AssetMasterGenerals>(request.AssetMaster);                   
            var updateResult = await _assetMasterGeneralRepository.UpdateAsync( request.AssetMaster.Id,updatedAssetMasterEntity);           
                
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.AssetMaster.AssetCode ?? string.Empty,
                actionName: request.AssetMaster.AssetName ?? string.Empty,                            
                details: $"AssetMaster '{oldAssetName}' was updated to '{request.AssetMaster.AssetName}'.  Code: {request.AssetMaster.AssetCode}",
                module:"AssetMasterGeneral"
            );            
            await _mediator.Publish(domainEvent, cancellationToken);
            if(updateResult)
            {
                string tempFilePath = request.AssetMaster.AssetImage;
                string tempDocumentPath = request.AssetMaster.AssetDocument;

                // ✅ Get company and unit names using lookup interfaces
                var companies = await _companyLookup.GetAllCompanyAsync();
                var units = await _unitLookup.GetAllUnitAsync();
                var companyMap = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                var unitMap = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var companyName = companyMap.TryGetValue(request.AssetMaster.CompanyId, out var cname) ? cname : string.Empty;
                var unitName = unitMap.TryGetValue(request.AssetMaster.UnitId, out var uname) ? uname : string.Empty;

                if (tempFilePath != null){

                    string baseDirectory = await _assetMasterGeneralQueryRepository.GetBaseDirectoryAsync();

                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName, unitName);     
                    string filePath = Path.Combine(uploadPath, tempFilePath);  
                    EnsureDirectoryExists(Path.GetDirectoryName(filePath));           

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                        string newFileName = $"{request.AssetMaster.AssetCode}{Path.GetExtension(tempFilePath)}";
                        string newFilePath = Path.Combine(directory, newFileName);

                        try
                        {
                            File.Move(filePath, newFilePath);
                            //assetEntity.AssetImage = newFileName;
                            await _assetMasterGeneralRepository.UpdateAssetImageAsync(request.AssetMaster.Id, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Information(ex, "Failed to rename file.");
                        }
                    }
                }  
                //Document
                if (tempDocumentPath != null){
                    string baseDirectory = await _assetMasterGeneralQueryRepository.GetDocumentDirectoryAsync();
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName, unitName);     
                    string filePath = Path.Combine(uploadPath, tempFilePath);  
                    EnsureDirectoryExists(Path.GetDirectoryName(filePath));           

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                        string newFileName = $"{request.AssetMaster.AssetCode}{Path.GetExtension(tempFilePath)}";
                        string newFilePath = Path.Combine(directory, newFileName);
                        try
                        {
                            File.Move(filePath, newFilePath);
                            //assetEntity.AssetImage = newFileName;
                            await _assetMasterGeneralRepository.UpdateAssetDocumentAsync(request.AssetMaster.Id, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Information(ex, "Failed to rename file: {ErrorMessage}", ex.Message);
                        }
                    }
                }       
                
                return true;
            }
            throw new Exception("AssetMaster not updated.");
                                   
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