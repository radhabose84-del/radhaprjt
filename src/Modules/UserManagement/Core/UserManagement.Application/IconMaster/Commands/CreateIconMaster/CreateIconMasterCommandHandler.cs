#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Commands.CreateIconMaster
{
    public class CreateIconMasterCommandHandler : IRequestHandler<CreateIconMasterCommand, int>
    {
        private readonly IIconMasterCommandRepository _iconMasterCommandRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _Imediator;
        private readonly ILogger<CreateIconMasterCommandHandler> _logger;

        public CreateIconMasterCommandHandler(IIconMasterCommandRepository iconMasterCommandRepository, IMapper Imapper, IMediator Imediator, ILogger<CreateIconMasterCommandHandler> logger)
        {
            _iconMasterCommandRepository = iconMasterCommandRepository;
            _Imapper = Imapper;
            _Imediator = Imediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(CreateIconMasterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting creation process for IconMaster: {request.Keyword}");

            // Keyword is the unique, immutable matching key
            var exists = await _iconMasterCommandRepository.ExistsByKeywordAsync(request.Keyword);
            if (exists)
            {
                _logger.LogWarning($"Icon keyword {request.Keyword} already exists.");
                throw new ValidationException("Icon keyword already exists.");
            }

            var iconMaster = _Imapper.Map<UserManagement.Domain.Entities.IconMaster>(request);

            var result = await _iconMasterCommandRepository.CreateAsync(iconMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: iconMaster.Keyword,
                actionName: iconMaster.IconName,
                details: $"IconMaster details was created",
                module: "IconMaster");
            await _Imediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"IconMaster {iconMaster.Keyword} Created successfully.");

            if (result > 0)
            {
                return result;
            }
            throw new Exception("IconMaster Creation Failed");
        }
    }
}
