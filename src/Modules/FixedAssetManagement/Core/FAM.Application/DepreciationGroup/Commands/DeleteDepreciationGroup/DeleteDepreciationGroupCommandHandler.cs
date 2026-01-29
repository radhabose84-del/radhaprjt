using AutoMapper;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup
{
    public class DeleteDepreciationGroupCommandHandler : IRequestHandler<DeleteDepreciationGroupCommand, DepreciationGroupDTO>
    {
        private readonly IDepreciationGroupCommandRepository _depreciationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IDepreciationGroupQueryRepository _depreciationQueryRepository;
        
        public DeleteDepreciationGroupCommandHandler(IDepreciationGroupCommandRepository depreciationRepository, IMapper mapper,  IMediator mediator,IDepreciationGroupQueryRepository depreciationQueryRepository)
        {
            _depreciationRepository = depreciationRepository;
            _mapper = mapper;        
            _mediator = mediator;
            _depreciationQueryRepository=depreciationQueryRepository;
        }

        public async Task<DepreciationGroupDTO> Handle(DeleteDepreciationGroupCommand request, CancellationToken cancellationToken)
        {             
            var depreciationGroups = await _depreciationQueryRepository.GetByIdAsync(request.Id);
            if (depreciationGroups is null)
                    throw new EntityNotFoundException("DepreciationGroup", request.Id);
            
            var depreciationDelete = _mapper.Map<DepreciationGroups>(request);      
            var updateResult = await _depreciationRepository.DeleteAsync(request.Id, depreciationDelete);

            if (updateResult <= 0)
                throw new ExceptionRules("DepreciationGroup deletion failed.");
          
            var depreciationGroupDto = _mapper.Map<DepreciationGroupDTO>(depreciationDelete);  
            //Domain Event  
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: depreciationDelete.Code ?? string.Empty,
                actionName: depreciationDelete.DepreciationGroupName ?? string.Empty,
                details: $"DepreciationGroup '{depreciationGroupDto.DepreciationGroupName}' was created. Code: {depreciationGroupDto.Code}",
                module:"DepreciationGroup"
            );               
            await _mediator.Publish(domainEvent, cancellationToken);        
            return depreciationGroupDto;
        }
    }
}