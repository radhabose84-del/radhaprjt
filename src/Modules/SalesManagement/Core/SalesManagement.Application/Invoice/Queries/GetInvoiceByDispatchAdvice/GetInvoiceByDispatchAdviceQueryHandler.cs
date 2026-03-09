using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceByDispatchAdvice
{
    public class GetInvoiceByDispatchAdviceQueryHandler : IRequestHandler<GetInvoiceByDispatchAdviceQuery, ApiResponseDTO<List<InvoiceHeaderDto>>>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetInvoiceByDispatchAdviceQueryHandler(IInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<InvoiceHeaderDto>>> Handle(GetInvoiceByDispatchAdviceQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByDispatchAdviceAsync(request.DispatchAdviceId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByDispatchAdvice",
                actionCode: "GetInvoiceByDispatchAdviceQuery",
                actionName: data.Count.ToString(),
                details: $"Invoices for DispatchAdvice {request.DispatchAdviceId} fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<InvoiceHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
