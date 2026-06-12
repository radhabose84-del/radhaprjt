using AutoMapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Queries.GetIconMasterById
{
    public class GetIconMasterByIdQueryHandler : IRequestHandler<GetIconMasterByIdQuery, IconMasterDto>
    {
        private readonly IIconMasterQueryRepository _iconMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetIconMasterByIdQueryHandler> _logger;

        public GetIconMasterByIdQueryHandler(IIconMasterQueryRepository iconMasterQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetIconMasterByIdQueryHandler> logger)
        {
            _iconMasterQueryRepository = iconMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IconMasterDto> Handle(GetIconMasterByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching IconMaster Request started: {request.IconMasterId}");
            var icon = await _iconMasterQueryRepository.GetByIdAsync(request.IconMasterId);
            if (icon is null)
            {
                _logger.LogWarning($"No IconMaster Record {request.IconMasterId} found in DB.");
                throw new ValidationException($"IconMaster Id {request.IconMasterId} not found.");
            }
            var iconDto = _mapper.Map<IconMasterDto>(icon);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetIconMasterByIdQuery",
                actionCode: "Get IconMaster",
                actionName: "",
                details: $"IconMaster details was fetched.",
                module: "IconMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            return iconDto;
        }
    }
}
