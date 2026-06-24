using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetAllJournal
{
    public class GetAllJournalQueryHandler : IRequestHandler<GetAllJournalQuery, ApiResponseDTO<List<JournalHeaderDto>>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllJournalQueryHandler(IJournalQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalHeaderDto>>> Handle(GetAllJournalQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId);

            var dtos = _mapper.Map<List<JournalHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllJournalQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal voucher details were fetched.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalHeaderDto>>
            {
                IsSuccess = true,
                Message = "Journal voucher list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
