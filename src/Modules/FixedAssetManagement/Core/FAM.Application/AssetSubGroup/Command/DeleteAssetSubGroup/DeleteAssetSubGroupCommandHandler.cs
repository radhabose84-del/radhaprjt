using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup
{
    public class DeleteAssetSubGroupCommandHandler : IRequestHandler<DeleteAssetSubGroupCommand, int>
    {
        private readonly IAssetSubGroupCommandRepository _iAssetSubGroupCommandRepository;
        private readonly IAssetSubGroupQueryRepository _iAssetSubGroupQueryRepository;
        private readonly IMapper _IMapper;
        private readonly ILogger<DeleteAssetSubGroupCommandHandler> _logger;
        private readonly IMediator _mediator; 

        public DeleteAssetSubGroupCommandHandler(IAssetSubGroupCommandRepository iAssetSubGroupCommandRepository, IMapper iMapper, ILogger<DeleteAssetSubGroupCommandHandler> logger, IMediator mediator, IAssetSubGroupQueryRepository iAssetSubGroupQueryRepository)
        {
            _iAssetSubGroupCommandRepository = iAssetSubGroupCommandRepository;
            _IMapper = iMapper;
            _logger = logger;
            _mediator = mediator;
            _iAssetSubGroupQueryRepository = iAssetSubGroupQueryRepository;
        }

        public async Task<int> Handle(DeleteAssetSubGroupCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting DeleteAssetSubGroupCommandHandler for request: {request}");

            var linked = await _iAssetSubGroupQueryRepository.IsAssetSubGroupLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }
            // 🔹 First, check if the ID exists in the database
            var existingAssetSubGroup = await _iAssetSubGroupQueryRepository.GetByIdAsync(request.Id);
            if (existingAssetSubGroup is null)
            {
                _logger.LogWarning($"AssetSubGroup ID {request.Id} not found.");
                throw new ValidationException("AssetSubGroup Id not found / AssetSubGroup is deleted .");
               
            }

            var assetSubGroup = _IMapper.Map<FAM.Domain.Entities.AssetSubGroup>(request);
            var result = await _iAssetSubGroupCommandRepository.DeleteAsync(request.Id,assetSubGroup);
            if (result == -1) 
            {
            _logger.LogInformation($"AssetSubGroup {request.Id} not found.");
            throw new ValidationException("AssetSubGroupId not found.");
             
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetSubGroup.Code??string.Empty,
                actionName: assetSubGroup.SubGroupName??string.Empty,
                details: $"AssetSubGroup details was deleted",
                module: "AssetSubGroup");
            await _mediator.Publish(domainEvent);
            _logger.LogInformation($"AssetSubGroup {assetSubGroup.SubGroupName} Deleted successfully.");

            return  result;
        }
    }
}