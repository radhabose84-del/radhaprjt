using AutoMapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Queries.GetIconMasterAutoComplete
{
    public class GetIconMasterAutocompleteQueryHandler : IRequestHandler<GetIconMasterAutocompleteQuery, List<IconMasterAutoCompleteDto>>
    {
        private readonly IIconMasterQueryRepository _iconMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetIconMasterAutocompleteQueryHandler> _logger;

        public GetIconMasterAutocompleteQueryHandler(IIconMasterQueryRepository iconMasterQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetIconMasterAutocompleteQueryHandler> logger)
        {
            _iconMasterQueryRepository = iconMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<IconMasterAutoCompleteDto>> Handle(GetIconMasterAutocompleteQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching IconMaster autocomplete: {request.SearchPattern}");
            var icons = await _iconMasterQueryRepository.GetByKeywordAsync(request.SearchPattern ?? string.Empty);
            var iconList = _mapper.Map<List<IconMasterAutoCompleteDto>>(icons);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetIconMasterAutocompleteQuery",
                actionCode: "Get IconMaster Autocomplete",
                actionName: iconList.Count.ToString(),
                details: $"IconMaster details was fetched.",
                module: "IconMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            return iconList;
        }
    }
}
