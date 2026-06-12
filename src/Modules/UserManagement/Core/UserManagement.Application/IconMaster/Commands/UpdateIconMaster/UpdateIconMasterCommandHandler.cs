#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Commands.UpdateIconMaster
{
    public class UpdateIconMasterCommandHandler : IRequestHandler<UpdateIconMasterCommand, int>
    {
        private readonly IIconMasterCommandRepository _iconMasterCommandRepository;
        private readonly IIconMasterQueryRepository _iconMasterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<UpdateIconMasterCommandHandler> _logger;
        private readonly IMediator _mediator;

        public UpdateIconMasterCommandHandler(IIconMasterCommandRepository iconMasterCommandRepository, IMapper Imapper, ILogger<UpdateIconMasterCommandHandler> logger, IMediator mediator, IIconMasterQueryRepository iconMasterQueryRepository)
        {
            _iconMasterCommandRepository = iconMasterCommandRepository;
            _Imapper = Imapper;
            _logger = logger;
            _mediator = mediator;
            _iconMasterQueryRepository = iconMasterQueryRepository;
        }

        public async Task<int> Handle(UpdateIconMasterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting IconMaster Update process for Id: {request.Id}");

            var existingIcon = await _iconMasterQueryRepository.GetByIdAsync(request.Id);
            if (existingIcon is null)
            {
                _logger.LogWarning($"IconMaster Id {request.Id} not found.");
                throw new ValidationException("IconMaster Id not found / IconMaster is deleted.");
            }

            var iconMaster = _Imapper.Map<UserManagement.Domain.Entities.IconMaster>(request);

            var result = await _iconMasterCommandRepository.UpdateAsync(request.Id, iconMaster);

            if (result == -1)
            {
                _logger.LogInformation($"IconMaster Id {request.Id} not found.");
                throw new ValidationException("IconMaster Id not found.");
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.Id.ToString(),
                actionName: iconMaster.IconName,
                details: $"IconMaster '{request.Id}' was Updated.",
                module: "IconMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"IconMaster {request.Id} Updated successfully.");

            return result;
        }
    }
}
