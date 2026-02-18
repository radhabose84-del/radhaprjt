
using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral
{
    public class DeleteAssetMasterGeneralCommandHandler : IRequestHandler<DeleteAssetMasterGeneralCommand, AssetMasterGeneralDTO>
    {
        private readonly IAssetMasterGeneralCommandRepository _assetMasterGeneralRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IAssetMasterGeneralQueryRepository _assetMasterGeneralQueryRepository;
        
        public DeleteAssetMasterGeneralCommandHandler(IAssetMasterGeneralCommandRepository assetMasterGeneralRepository, IMapper mapper,  IMediator mediator,IAssetMasterGeneralQueryRepository assetMasterGeneralQueryRepository)
        {
            _assetMasterGeneralRepository = assetMasterGeneralRepository;
             _mapper = mapper;        
            _mediator = mediator;
            _assetMasterGeneralQueryRepository=assetMasterGeneralQueryRepository;
        }

        public async Task<AssetMasterGeneralDTO> Handle(DeleteAssetMasterGeneralCommand request, CancellationToken cancellationToken)
        {
            var assetMasterGeneral = await _assetMasterGeneralQueryRepository.GetByIdAsync(request.Id);
            if (assetMasterGeneral is null)
                    throw new EntityNotFoundException("AssetMaster", request.Id);
          
            var assetMasterDelete = _mapper.Map<AssetMasterGenerals>(request);      
            var updateResult = await _assetMasterGeneralRepository.DeleteAsync(request.Id, assetMasterDelete);

             if (!updateResult)
                throw new ExceptionRules("AssetMaster deletion failed.");
        
            var assetMasterDto = _mapper.Map<AssetMasterGeneralDTO>(assetMasterDelete);                  
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetMasterDelete.AssetCode ?? string.Empty,
                actionName: assetMasterDelete.AssetName ?? string.Empty,
                details: $"AssetMaster '{assetMasterDto.AssetName}' was created. Code: {assetMasterDto.AssetCode}",
                module:"AssetMaster"
            );               
            await _mediator.Publish(domainEvent, cancellationToken);                 
            return assetMasterDto;    
        }
    }
}