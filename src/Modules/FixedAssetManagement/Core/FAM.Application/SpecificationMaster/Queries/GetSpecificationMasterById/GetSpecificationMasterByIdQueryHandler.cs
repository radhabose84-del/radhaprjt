using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterById
{
    public class GetSpecificationMasterByIdQueryHandler : IRequestHandler<GetSpecificationMasterByIdQuery, SpecificationMasterDTO>
    {
        private readonly ISpecificationMasterQueryRepository _specificationMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetSpecificationMasterByIdQueryHandler(ISpecificationMasterQueryRepository specificationMasterRepository,  IMapper mapper, IMediator mediator)
        {
            _specificationMasterRepository =specificationMasterRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<SpecificationMasterDTO> Handle(GetSpecificationMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var specificationMaster = await _specificationMasterRepository.GetByIdAsync(request.Id);                
            var specificationMasterDto = _mapper.Map<SpecificationMasterDTO>(specificationMaster);
            if (specificationMaster is null)
            {      
                throw new ValidationException("SpecificationMaster with ID {request.Id} not found.");          
                   
            }       
                //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: specificationMasterDto.AssetGroupId.ToString() ?? string.Empty,        
                actionName: specificationMasterDto.SpecificationName ?? string.Empty,                
                details: $"SpecificationMaster '{specificationMasterDto.SpecificationName}' was created",
                module:"SpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return specificationMasterDto;       
        }
    }
}