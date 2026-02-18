using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup
{
    public class CreateAssetSubGroupCommandHandler : IRequestHandler<CreateAssetSubGroupCommand, int>
    {
        private readonly IAssetSubGroupCommandRepository _iAssetSubGroupCommandRepository;
        private readonly IMediator _iMediator;
        private readonly IMapper _iMapper;
        private readonly ILogger<CreateAssetSubGroupCommandHandler> _logger;

        public CreateAssetSubGroupCommandHandler(IAssetSubGroupCommandRepository IAssetSubGroupCommandRepository, IMediator iMediator, IMapper iMapper, ILogger<CreateAssetSubGroupCommandHandler> logger)
        {
            _iAssetSubGroupCommandRepository = IAssetSubGroupCommandRepository;
            _iMediator = iMediator;
            _iMapper = iMapper;
            _logger = logger;
        }
        public async Task<int> Handle(CreateAssetSubGroupCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting creation process for AssetSubGroup: {request}");
             // Check if AssetSubGroup code already exists
            var exists = await _iAssetSubGroupCommandRepository.ExistsByCodeAsync(request.Code??string.Empty);
            if (exists)
            {
                 _logger.LogWarning($"AssetSubGroup Code {request.Code} already exists.");
                 throw new ValidationException("AssetSubGroup Code already exists.");
               
            }
            var assetSubGroup = _iMapper.Map<FAM.Domain.Entities.AssetSubGroup>(request);
            
            var result = await _iAssetSubGroupCommandRepository.CreateAsync(assetSubGroup);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetSubGroup.Code??string.Empty,
                actionName: assetSubGroup.SubGroupName??string.Empty,
                details: $"assetSubGroup details was created",
                module: "assetSubGroup");
            await _iMediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"assetSubGroup {assetSubGroup.SubGroupName} Created successfully.");
            var assetSubGroupDto = _iMapper.Map<AssetSubGroupDto>(assetSubGroup);
            if (result > 0)
                  {
                     _logger.LogInformation($"AssetSubGroupId {result} created successfully");
                        return  result;
                 }
                 throw new Exception("AssetSubGroup Creation Failed");
           
        }
    }
}