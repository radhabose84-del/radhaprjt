using AutoMapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Commands.DeleteIconMaster
{
    public class DeleteIconMasterCommandHandler : IRequestHandler<DeleteIconMasterCommand, int>
    {
        private readonly IIconMasterCommandRepository _iconMasterCommandRepository;
        private readonly IIconMasterQueryRepository _iconMasterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _Imediator;
        private readonly ILogger<DeleteIconMasterCommandHandler> _logger;

        public DeleteIconMasterCommandHandler(IIconMasterCommandRepository iconMasterCommandRepository, IMapper Imapper, IMediator Imediator, ILogger<DeleteIconMasterCommandHandler> logger, IIconMasterQueryRepository iconMasterQueryRepository)
        {
            _iconMasterCommandRepository = iconMasterCommandRepository;
            _Imapper = Imapper;
            _Imediator = Imediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iconMasterQueryRepository = iconMasterQueryRepository;
        }

        public async Task<int> Handle(DeleteIconMasterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Soft Deleting IconMaster with Id: {request.Id}");

            var icon = await _iconMasterQueryRepository.GetByIdAsync(request.Id);
            if (icon is null)
            {
                _logger.LogWarning($"Soft Deleting IconMaster Failed: Id {request.Id} not found.");
                throw new ValidationException("IconMaster not found / IconMaster is deleted.");
            }

            var iconDelete = _Imapper.Map<UserManagement.Domain.Entities.IconMaster>(request);

            var result = await _iconMasterCommandRepository.DeleteIconMasterAsync(request.Id, iconDelete);
            if (result == -1)
            {
                _logger.LogWarning($"Soft Deleting IconMaster Failed with Id: {request.Id}");
                throw new ValidationException("IconMaster not found.");
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: request.Id.ToString(),
                actionName: "DeleteIconMasterCommand",
                details: $"IconMaster '{request.Id}' was Deleted.",
                module: "IconMaster");
            await _Imediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Soft Deleting IconMaster Successfully Completed with Id: {request.Id}");
            return request.Id;
        }
    }
}
