#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance
{
    public class UpdateAssetInsuranceCommandHandler  : IRequestHandler<UpdateAssetInsuranceCommand, bool>
    {
    private readonly IAssetInsuranceCommandRepository _assetInsuranceCommandRepository;
    private readonly IAssetInsuranceQueryRepository _assetInsuranceQueryRepository;
    private readonly IMapper _imapper;
    private readonly IMediator _mediator;
    public UpdateAssetInsuranceCommandHandler(IAssetInsuranceCommandRepository assetInsuranceCommandRepository,IAssetInsuranceQueryRepository assetInsuranceQueryRepository, IMapper mapper, IMediator mediator)
    {
        _assetInsuranceCommandRepository = assetInsuranceCommandRepository;
        _assetInsuranceQueryRepository = assetInsuranceQueryRepository;
        _imapper = mapper;
        _mediator = mediator;

    }

     public async Task<bool> Handle(UpdateAssetInsuranceCommand request, CancellationToken cancellationToken)
    {

        var assetInsurance = await _assetInsuranceQueryRepository.GetByAssetIdAsync(request.Id);
                 if (assetInsurance == null)
                {
                    throw new ValidationException("AssetInsurance not found.");
                }
                 var mAssetInsurancemap  = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(request);         
                var AssetInsuranceresult = await _assetInsuranceCommandRepository.UpdateAsync(request.Id, mAssetInsurancemap);                

                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: mAssetInsurancemap.AssetId.ToString(),
                        actionName: mAssetInsurancemap.PolicyNo,
                        details: $"AssetInsurance '{mAssetInsurancemap.Id}' was updated.",
                        module:"AssetInsurance"
                    );    

                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(AssetInsuranceresult)
                {
                    return AssetInsuranceresult;
                }
                throw new Exception("AssetInsurance  not updated.");

    }

        
    }
}