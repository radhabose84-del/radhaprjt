using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetAllInvoice
{
    public class GetAllInvoiceQueryHandler : IRequestHandler<GetAllInvoiceQuery, ApiResponseDTO<List<InvoiceHeaderDto>>>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllInvoiceQueryHandler(IInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<InvoiceHeaderDto>>> Handle(GetAllInvoiceQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.Status);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllInvoiceQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Invoice details were fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<InvoiceHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
