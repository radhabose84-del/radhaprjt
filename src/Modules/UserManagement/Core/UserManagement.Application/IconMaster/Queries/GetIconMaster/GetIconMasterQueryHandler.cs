using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.IconMaster.Queries.GetIconMaster
{
    public class GetIconMasterQueryHandler : IRequestHandler<GetIconMasterQuery, ApiResponseDTO<List<IconMasterDto>>>
    {
        private readonly IIconMasterQueryRepository _iconMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetIconMasterQueryHandler> _logger;

        public GetIconMasterQueryHandler(IIconMasterQueryRepository iconMasterQueryRepository, IMapper mapper, IMediator mediator, ILogger<GetIconMasterQueryHandler> logger)
        {
            _iconMasterQueryRepository = iconMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponseDTO<List<IconMasterDto>>> Handle(GetIconMasterQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching IconMaster Request started.");
            var (icons, totalCount) = await _iconMasterQueryRepository.GetAllIconMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var iconList = _mapper.Map<List<IconMasterDto>>(icons);
            _logger.LogInformation($"Fetching IconMaster Request Completed: {iconList.Count}");

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetIconMasterQuery",
                actionCode: "Get IconMaster",
                actionName: iconList.Count.ToString(),
                details: $"IconMaster details was fetched.",
                module: "IconMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<IconMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = iconList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
