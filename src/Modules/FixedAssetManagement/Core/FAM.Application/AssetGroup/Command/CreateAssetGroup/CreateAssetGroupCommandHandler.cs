#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetGroup.Command.CreateAssetGroup
{
    public class CreateAssetGroupCommandHandler : IRequestHandler<CreateAssetGroupCommand, int>
    {
        private readonly IAssetGroupCommandRepository _iAssetGroupCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly ILogger<CreateAssetGroupCommandHandler> _logger;

        public CreateAssetGroupCommandHandler(IAssetGroupCommandRepository IAssetGroupCommandRepository, IMediator imediator, IMapper imapper, ILogger<CreateAssetGroupCommandHandler> logger)
        {
            _iAssetGroupCommandRepository = IAssetGroupCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
            _logger = logger;
        }
        public async Task<int> Handle(CreateAssetGroupCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting creation process for AssetGroup: {request}");
             // Check if AssetGroup code already exists
            var exists = await _iAssetGroupCommandRepository.ExistsByCodeAsync(request.Code);
            if (exists)
            {
                 _logger.LogWarning($"AssetGroup Code {request.Code} already exists.");
                throw new ValidationException("AssetGroup Code already exists.");
                
            }
            var assetGroup = _imapper.Map<FAM.Domain.Entities.AssetGroup>(request);
            
            var result = await _iAssetGroupCommandRepository.CreateAsync(assetGroup);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetGroup.Code,
                actionName: assetGroup.GroupName,
                details: $"AssetGroup details was created",
                module: "AssetGroup");
            await _imediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AssetGroup {assetGroup.GroupName} Created successfully.");
            
            if (result > 0)
                  {
                     _logger.LogInformation($"AssetGroupId {result} created successfully");
                        return  result;
                 }

                 throw new Exception("AssetGroup Creation Failed");
           
        }
    }
}