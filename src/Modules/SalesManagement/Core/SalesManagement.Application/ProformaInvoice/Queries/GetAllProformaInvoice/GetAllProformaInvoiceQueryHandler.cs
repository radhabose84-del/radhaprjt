using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetAllProformaInvoice
{
    public class GetAllProformaInvoiceQueryHandler : IRequestHandler<GetAllProformaInvoiceQuery, ApiResponseDTO<List<ProformaInvoiceDto>>>
    {
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllProformaInvoiceQueryHandler(IProformaInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProformaInvoiceDto>>> Handle(GetAllProformaInvoiceQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllProformaInvoiceQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "ProformaInvoice details were fetched.",
                module: "ProformaInvoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProformaInvoiceDto>>
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
