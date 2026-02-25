using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster
{
    public class UpdateSpecificationMasterCommandHandler : IRequestHandler<UpdateSpecificationMasterCommand, SpecificationMasterDTO>
    {
        private readonly ISpecificationMasterCommandRepository _specificationMasterRepository;
        private readonly ISpecificationMasterQueryRepository _specificationMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public UpdateSpecificationMasterCommandHandler(ISpecificationMasterCommandRepository specificationMasterRepository, IMapper mapper,ISpecificationMasterQueryRepository specificationMasterQueryRepository, IMediator mediator)
        {
            _specificationMasterRepository = specificationMasterRepository;
            _mapper = mapper;
            _specificationMasterQueryRepository = specificationMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<SpecificationMasterDTO> Handle(UpdateSpecificationMasterCommand request, CancellationToken cancellationToken)
        {
            var specificationMaster = await _specificationMasterQueryRepository.GetByIdAsync(request.Id);
            if (specificationMaster is null)
            throw new ValidationException("Invalid SpecificationMaster. The specified Name does not exist or is inactive.");
           
           var linked = await _specificationMasterQueryRepository.IsSpecificationMasterLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }
            
            var oldSpecificationMaster = specificationMaster.SpecificationName;
            specificationMaster.SpecificationName = request.SpecificationName;

           
            var updatedSpecificationEntity= _mapper.Map<SpecificationMasters>(request);                   
            var updateResult = await _specificationMasterRepository.UpdateAsync(updatedSpecificationEntity);            

            var updatedSpecificationMaster =  await _specificationMasterQueryRepository.GetByIdAsync(request.Id);    
            if (updatedSpecificationMaster != null)
            {
                var specificationMasterDto = _mapper.Map<SpecificationMasterDTO>(updatedSpecificationMaster);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: specificationMasterDto.AssetGroupId.ToString()??string.Empty,
                    actionName: specificationMasterDto.SpecificationName ?? string.Empty,                            
                    details: $"SpecificationMaster '{oldSpecificationMaster}' was updated to '{specificationMasterDto.SpecificationName}'",
                    module:"SpecificationMaster"
                );            
                await _mediator.Publish(domainEvent, cancellationToken);
                if(updateResult>0)
                {
                    return  specificationMasterDto;
                }
                throw new Exception("SpecificationMaster not updated.");
                           
            }
            else
            {
                throw new ValidationException("SpecificationMaster not found.");
              
            }
        }
    }
}