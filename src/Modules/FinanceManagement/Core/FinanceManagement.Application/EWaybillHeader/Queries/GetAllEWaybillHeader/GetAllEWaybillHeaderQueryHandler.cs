using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetAllEWaybillHeader
{
    public class GetAllEWaybillHeaderQueryHandler : IRequestHandler<GetAllEWaybillHeaderQuery, ApiResponseDTO<List<EWaybillHeaderDto>>>
    {
        private readonly IEWaybillHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllEWaybillHeaderQueryHandler(
            IEWaybillHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<EWaybillHeaderDto>>> Handle(GetAllEWaybillHeaderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<EWaybillHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllEWaybillHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "EWaybill Header details were fetched.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<EWaybillHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
