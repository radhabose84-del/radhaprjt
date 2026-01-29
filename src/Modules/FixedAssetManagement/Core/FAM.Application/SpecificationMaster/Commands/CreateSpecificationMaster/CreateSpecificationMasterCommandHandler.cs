
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster
{
    public class CreateSpecificationMasterCommandHandler : IRequestHandler<CreateSpecificationMasterCommand, SpecificationMasterDTO>
    {        
        private readonly IMapper _mapper;
        private readonly ISpecificationMasterCommandRepository _specificationMasterRepository;
        private readonly IMediator _mediator;

        public CreateSpecificationMasterCommandHandler(IMapper mapper, ISpecificationMasterCommandRepository specificationMasterRepository, IMediator mediator)
        {
            _mapper = mapper;
            _specificationMasterRepository = specificationMasterRepository;
            _mediator = mediator;    
        } 

       public async Task<SpecificationMasterDTO> Handle(CreateSpecificationMasterCommand request, CancellationToken cancellationToken)
        {
            var specificationMasterExists = await _specificationMasterRepository.ExistsByAssetGroupIdAsync(request.AssetGroupId,request.SpecificationName);
            if (specificationMasterExists)
            {
                throw new ValidationException("SpecificationMaster already exists.");
                               
            }
       
            var specificationMasterEntity = _mapper.Map<SpecificationMasters>(request);            
            var result = await _specificationMasterRepository.CreateAsync(specificationMasterEntity);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: specificationMasterEntity.AssetGroupId.ToString(),
                actionName: specificationMasterEntity.SpecificationName ?? string.Empty,
                details: $"SpecificationMaster '{specificationMasterEntity.SpecificationName}' was created.",
                module:"SpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            var specificationMasterDto = _mapper.Map<SpecificationMasterDTO>(result);
            if (specificationMasterDto.Id > 0)
            {
                return  specificationMasterDto;
            }
            throw new Exception("SpecificationMaster not created.");
                 
        }
    }
}