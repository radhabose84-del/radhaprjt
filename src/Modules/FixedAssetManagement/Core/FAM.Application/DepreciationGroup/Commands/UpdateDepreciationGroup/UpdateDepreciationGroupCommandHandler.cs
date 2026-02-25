using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup
{
    public class UpdateDepreciationGroupCommandHandler : IRequestHandler<UpdateDepreciationGroupCommand, bool>
    {
        private readonly IDepreciationGroupCommandRepository _depreciationGroupRepository;
        private readonly IDepreciationGroupQueryRepository _depreciationGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public UpdateDepreciationGroupCommandHandler(IDepreciationGroupCommandRepository depreciationGroupRepository, IMapper mapper,IDepreciationGroupQueryRepository depreciationGroupQueryRepository, IMediator mediator)
        {
            _depreciationGroupRepository = depreciationGroupRepository;
            _mapper = mapper;
            _depreciationGroupQueryRepository = depreciationGroupQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateDepreciationGroupCommand request, CancellationToken cancellationToken)
        {
            var depreciationGroups = await _depreciationGroupQueryRepository.GetByIdAsync(request.Id);
            if (depreciationGroups is null)
                throw new EntityNotFoundException("DepreciationGroup", request.Id);
            
            // Step 2: Check for duplicates
            var isDuplicate = await _depreciationGroupRepository.CheckForDuplicatesAsync(
                request.AssetGroupId,
                request.DepreciationMethod,
                request.BookType,                                           
                request.Id);

             if (isDuplicate != null)
            {
                if (isDuplicate.IsActive == BaseEntity.Status.Active)
                    throw new EntityAlreadyExistsException("DepreciationGroup", "Combination", "Already exists.");
                else
                    throw new EntityAlreadyExistsException("DepreciationGroup", "Combination", "Already exists but status is inactive.");
            }
            
            var oldDepreciationName = depreciationGroups.DepreciationGroupName;            
            var updatedDepreciationEntity = _mapper.Map<DepreciationGroups>(request);                   
            var updateResult = await _depreciationGroupRepository.UpdateAsync(updatedDepreciationEntity);            
            if (!updateResult)
                throw new ExceptionRules("DepreciationGroup update failed.");
          
                //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.Code ?? string.Empty,
                actionName: request.DepreciationGroupName ?? string.Empty,                            
                details: $"DepreciationGroup '{oldDepreciationName}' was updated to '{request.DepreciationGroupName}'.  Code: {request.Code}",
                module:"DepreciationGroup"
            );            
            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }         
    }
}