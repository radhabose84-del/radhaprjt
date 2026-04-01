using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderById
{
    public class GetYarnConversionHeaderByIdQueryHandler
        : IRequestHandler<GetYarnConversionHeaderByIdQuery, YarnConversionHeaderDto?>
    {
        private readonly IYarnConversionHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnConversionHeaderByIdQueryHandler(
            IYarnConversionHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<YarnConversionHeaderDto?> Handle(
            GetYarnConversionHeaderByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetYarnConversionHeaderByIdQuery",
                actionName: result.Id.ToString(),
                details: $"YarnConversionHeader details {result.Id} was fetched.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
