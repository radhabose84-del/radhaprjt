using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence
{
    public class GetAllDocumentSequenceQueryHandler : IRequestHandler<GetAllDocumentSequenceQuery, ApiResponseDTO<List<DocumentSequenceDto>>>
    {
        private readonly IDocumentSequenceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDocumentSequenceQueryHandler(
            IDocumentSequenceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DocumentSequenceDto>>> Handle(GetAllDocumentSequenceQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<DocumentSequenceDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDocumentSequenceQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Document Sequence details were fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DocumentSequenceDto>>
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
