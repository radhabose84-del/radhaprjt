using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Queries.GetSalesReturnById
{
    public class GetSalesReturnByIdQueryHandler : IRequestHandler<GetSalesReturnByIdQuery, ApiResponseDTO<SalesReturnHeaderDto>>
    {
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesReturnByIdQueryHandler(ISalesReturnQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesReturnHeaderDto>> Handle(GetSalesReturnByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);

            if (data == null)
                return new ApiResponseDTO<SalesReturnHeaderDto>
                {
                    IsSuccess = false,
                    Message = "Sales Return not found.",
                    Data = null
                };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesReturnByIdQuery",
                actionName: data.Id.ToString(),
                details: $"Sales Return {data.Id} was fetched.",
                module: "SalesReturn");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesReturnHeaderDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
