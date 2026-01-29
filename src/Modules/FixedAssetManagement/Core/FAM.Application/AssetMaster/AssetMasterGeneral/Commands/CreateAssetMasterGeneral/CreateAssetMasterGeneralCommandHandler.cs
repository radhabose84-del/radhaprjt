using AutoMapper;
// using Contracts.Events.Users;
// using Contracts.Interfaces.External.IUser;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;
using Serilog;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral
{
    public class CreateAssetMasterGeneralCommandHandler : IRequestHandler<CreateAssetMasterGeneralCommand, AssetMasterDto>
    {
        private readonly IMapper _mapper;
        private readonly IAssetMasterGeneralCommandRepository _assetMasterGeneralRepository;
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        private readonly IMediator _mediator;        
        // private readonly IUnitGrpcClient _unitGrpcClient;
        // private readonly ICompanyGrpcClient _companyGrpcClient;

        public CreateAssetMasterGeneralCommandHandler(IMapper mapper, IAssetMasterGeneralCommandRepository assetMasterGeneralRepository, IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository, IMediator mediator
        // , IUnitGrpcClient unitGrpcClient, ICompanyGrpcClient companyGrpcClient
        )
        {
            _mapper = mapper;
            _assetMasterGeneralRepository = assetMasterGeneralRepository;
            _assetMasterGeneralQueryRepository = assetMasterGeneralQueryRepository;
            _mediator = mediator;            
            // _unitGrpcClient = unitGrpcClient;
            // _companyGrpcClient = companyGrpcClient;
        }

        public async Task<AssetMasterDto> Handle(CreateAssetMasterGeneralCommand request, CancellationToken cancellationToken)
        {
            // Get latest AssetCode
            var latestAssetCode = await _assetMasterGeneralQueryRepository.GetLatestAssetCode( request.AssetMaster.AssetGroupId, request.AssetMaster.AssetCategoryId, request.AssetMaster.AssetLocation.DepartmentId, request.AssetMaster.AssetLocation.LocationId);
            var assetCode = latestAssetCode;
            var assetEntity = _mapper.Map<AssetMasterGenerals>(request.AssetMaster);
            assetEntity.AssetCode = assetCode;
            var result = await _assetMasterGeneralRepository.CreateAsync(assetEntity, cancellationToken);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetEntity.AssetCode ?? string.Empty,
                actionName: assetEntity.AssetName ?? string.Empty,
                details: $"AssetMasterGeneral '{assetEntity.AssetName}' was created. Code: {assetEntity.AssetCode}",
                module: "AssetMasterGeneral"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            var assetMasterDTO = _mapper.Map<AssetMasterDto>(result);
            if (result.Id > 0)
            {             
                string tempFilePath = request.AssetMaster.AssetImage;
                if (tempFilePath != null){
                    // var companies = await _companyGrpcClient.GetAllCompanyAsync();
                    // var units = await _unitGrpcClient.GetAllUnitAsync();
                    //  var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                    // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                    // var companyName = companyLookup.TryGetValue(request.AssetMaster.CompanyId, out var cname) ? cname : string.Empty;
                    // var unitName = unitLookup.TryGetValue(request.AssetMaster.UnitId, out var uname) ? uname : string.Empty;   

                    string baseDirectory = await _assetMasterGeneralQueryRepository.GetBaseDirectoryAsync();

                    //var (companyName, unitName) = await _assetMasterGeneralQueryRepository.GetCompanyUnitAsync(request.AssetMaster.CompanyId ,request.AssetMaster.UnitId);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
                    // , companyName, unitName
                    );   
                    string filePath = Path.Combine(uploadPath, tempFilePath);
                    EnsureDirectoryExists(Path.GetDirectoryName(filePath));             

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                        string newFileName = $"{result.AssetCode}{Path.GetExtension(tempFilePath)}";
                        string newFilePath = Path.Combine(directory, newFileName);
                        try
                        {
                            File.Move(filePath, newFilePath);                            
                            await _assetMasterGeneralRepository.UpdateAssetImageAsync(assetEntity.Id, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Information(ex, "Failed to rename file.");
                        }
                    }
                }                 
            }
            return assetMasterDTO;            
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