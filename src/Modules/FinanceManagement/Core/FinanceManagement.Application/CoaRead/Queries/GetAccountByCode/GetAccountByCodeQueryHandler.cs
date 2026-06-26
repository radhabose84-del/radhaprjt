using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.CoaRead.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.GetAccountByCode
{
    public class GetAccountByCodeQueryHandler : IRequestHandler<GetAccountByCodeQuery, ApiResponseDTO<CoaAccountReadDto?>>
    {
        private readonly ICoaReadQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAccountByCodeQueryHandler(
            ICoaReadQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<CoaAccountReadDto?>> Handle(GetAccountByCodeQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var account = await _queryRepository.GetByCodeAsync(companyId, request.AccountCode, cancellationToken);

            // AC4 — every call is logged.
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAccountByCodeQuery",
                actionCode: "Get",
                actionName: request.AccountCode,
                details: $"COA read get-by-code '{request.AccountCode}' ({(account == null ? "miss" : "hit")}).",
                module: "CoaRead"), cancellationToken);

            return new ApiResponseDTO<CoaAccountReadDto?>
            {
                IsSuccess = account != null,
                Message = account != null ? "Success" : "Account not found.",
                Data = account
            };
        }
    }
}
