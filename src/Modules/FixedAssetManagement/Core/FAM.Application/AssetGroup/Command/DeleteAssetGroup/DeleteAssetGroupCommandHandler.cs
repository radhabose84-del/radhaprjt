using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetGroup.Command.DeleteAssetGroup
{
    public class DeleteAssetGroupCommandHandler : IRequestHandler<DeleteAssetGroupCommand, int>
    {
        private readonly IAssetGroupCommandRepository _iAssetGroupCommandRepository;
        private readonly IAssetGroupQueryRepository _iAssetGroupQueryRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<DeleteAssetGroupCommandHandler> _logger;
        private readonly IMediator _mediator; 

        public DeleteAssetGroupCommandHandler(IAssetGroupCommandRepository iAssetGroupCommandRepository, IMapper imapper, ILogger<DeleteAssetGroupCommandHandler> logger, IMediator mediator, IAssetGroupQueryRepository iAssetGroupQueryRepository)
        {
            _iAssetGroupCommandRepository = iAssetGroupCommandRepository;
            _Imapper = imapper;
            _logger = logger;
            _mediator = mediator;
            _iAssetGroupQueryRepository = iAssetGroupQueryRepository;
        }

        public async Task<int> Handle(DeleteAssetGroupCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting DeleteAssetGroupCommandHandler for request: {request}");

            // 🔹 First, check if the ID exists in the database
            var existingAssetGroup = await _iAssetGroupQueryRepository.GetByIdAsync(request.Id);
            if (existingAssetGroup is null)
            {
                _logger.LogWarning($"AssetGroup ID {request.Id} not found.");

                throw new ValidationException("AssetGroup Id not found / AssetGroup is deleted .");
             
            }

            // DELETE BLOCK RULE: if referenced, block delete
            var isLinked = await _iAssetGroupQueryRepository.IsAssetGroupLinkedAsync(request.Id);
            if (isLinked)
            {
                throw new ValidationException("This master is linked with other records and cannot be deleted.");
            }
            
            var assetGroup = _Imapper.Map<FAM.Domain.Entities.AssetGroup>(request);
            var result = await _iAssetGroupCommandRepository.DeleteAsync(request.Id,assetGroup);
            if (result == -1) 
            {
            _logger.LogInformation($"AssetGroup {request.Id} not found.");
            throw new ValidationException("AssetGroupId not found.");
             
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetGroup.Code,
                actionName: assetGroup.GroupName,
                details: $"AssetGroup details was deleted",
                module: "AssetGroup");
            await _mediator.Publish(domainEvent);
            _logger.LogInformation($"AssetGroup {assetGroup.GroupName} Deleted successfully.");

            return result;
    
           
        }
    }
}