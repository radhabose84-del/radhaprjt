using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader
{
    public class GetAllEInvoiceHeaderQueryHandler : IRequestHandler<GetAllEInvoiceHeaderQuery, ApiResponseDTO<List<EInvoiceHeaderDto>>>
    {
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllEInvoiceHeaderQueryHandler(
            IEInvoiceHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<EInvoiceHeaderDto>>> Handle(GetAllEInvoiceHeaderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<EInvoiceHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllEInvoiceHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "EInvoice Header details were fetched.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<EInvoiceHeaderDto>>
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
