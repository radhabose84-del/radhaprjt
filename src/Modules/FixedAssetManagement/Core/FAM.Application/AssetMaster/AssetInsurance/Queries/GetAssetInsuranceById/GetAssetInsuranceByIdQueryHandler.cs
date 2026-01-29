using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById
{
    public class GetAssetInsuranceByIdQueryHandler  : IRequestHandler<GetAssetInsuranceByIdQuery, GetAssetInsuranceDto>
    {   

         private readonly IAssetInsuranceQueryRepository  _assetInsuranceQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        

        public GetAssetInsuranceByIdQueryHandler(IAssetInsuranceQueryRepository  assetInsuranceQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetInsuranceQueryRepository = assetInsuranceQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
       
       public async Task<GetAssetInsuranceDto> Handle(GetAssetInsuranceByIdQuery request, CancellationToken cancellationToken)
        {
            var assetInsurance = await _assetInsuranceQueryRepository.GetByAssetIdAsync(request.Id);
             var assetinsuranceDto = _mapper.Map<GetAssetInsuranceDto>(assetInsurance);
             if (assetInsurance is null)
            {         
                throw new ValidationException("AssetLocation with ID {request.Id} not found.");       
                  
            }      

              //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: assetinsuranceDto.AssetId == null ? "" : assetinsuranceDto.AssetId.ToString(),        
                actionName: assetinsuranceDto.PolicyNo == null ? "" : assetinsuranceDto.PolicyNo.ToString(),                
                details: $"Asset '{assetinsuranceDto.AssetId}' was created. Code: {assetinsuranceDto.Id}",
                module:"AssetMasterGeneral"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  assetinsuranceDto;       

        }
    }
}