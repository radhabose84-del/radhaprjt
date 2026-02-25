using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster
{
    public class DeleteSpecificationMasterCommandHandler : IRequestHandler<DeleteSpecificationMasterCommand, SpecificationMasterDTO>
    {
        private readonly ISpecificationMasterCommandRepository _specificationMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ISpecificationMasterQueryRepository _specificationMasterQueryRepository;
        
        public DeleteSpecificationMasterCommandHandler(ISpecificationMasterCommandRepository specificationMasterRepository, IMapper mapper,  IMediator mediator,ISpecificationMasterQueryRepository specificationMasterQueryRepository)
        {
            _specificationMasterRepository = specificationMasterRepository;
             _mapper = mapper;        
            _mediator = mediator;
            _specificationMasterQueryRepository=specificationMasterQueryRepository;
        }

        public async Task<SpecificationMasterDTO> Handle(DeleteSpecificationMasterCommand request, CancellationToken cancellationToken)
        {             
            var specificationMaster = await _specificationMasterQueryRepository.GetByIdAsync(request.Id);
            if (specificationMaster is null )
            {
                throw new ValidationException("Invalid SpecificationMasterID.");
               
            }
            var specificationMasterDelete = _mapper.Map<SpecificationMasters>(request); 
            var linked = await _specificationMasterQueryRepository.IsSpecificationMasterLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }
            var updateResult = await _specificationMasterRepository.DeleteAsync(request.Id, specificationMasterDelete);
            if (updateResult > 0)
            {
                var specificationMasterDto = _mapper.Map<SpecificationMasterDTO>(specificationMasterDelete);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: specificationMasterDelete.AssetGroupId.ToString(),
                    actionName: specificationMasterDelete.SpecificationName ?? string.Empty,
                    details: $"Specification Master '{specificationMasterDto.SpecificationName}' was created",
                    module:"Specification Master"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  specificationMasterDto;
            }
        throw new Exception("Specification Master deletion failed.");
                   
        }
    }
}