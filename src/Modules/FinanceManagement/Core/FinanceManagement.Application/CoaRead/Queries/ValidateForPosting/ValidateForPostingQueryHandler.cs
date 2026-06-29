using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.CoaRead.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.ValidateForPosting
{
    public class ValidateForPostingQueryHandler : IRequestHandler<ValidateForPostingQuery, ApiResponseDTO<PostingValidationResultDto>>
    {
        private readonly ICoaReadQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ValidateForPostingQueryHandler(
            ICoaReadQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<PostingValidationResultDto>> Handle(ValidateForPostingQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = new PostingValidationResultDto { AccountCode = request.AccountCode };
            var info = await _queryRepository.GetPostingInfoByCodeAsync(companyId, request.AccountCode, cancellationToken);

            if (info == null)
            {
                result.IsValid = false;
                result.Reasons.Add("Account not found for this company.");
            }
            else
            {
                result.AccountId = info.Id;

                // AC2 — inactive account cannot be posted to.
                if (!info.IsActive)
                    result.Reasons.Add("Account is inactive.");

                // AC2 — currency must match the account's configured currency (when a currency is supplied).
                if (request.CurrencyId is > 0 && request.CurrencyId.Value != info.CurrencyTypeId)
                    result.Reasons.Add($"Currency mismatch: account expects {info.CurrencyTypeCode ?? info.CurrencyTypeId.ToString()}.");

                // AC2 — cost centre required when the account mandates one.
                if (info.IsCostCentreMandatory && !(request.CostCentreId is > 0))
                    result.Reasons.Add("Cost centre is required for this account.");

                result.IsValid = result.Reasons.Count == 0;
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "ValidateForPostingQuery",
                actionCode: "Get",
                actionName: request.AccountCode,
                details: $"COA validate-for-posting '{request.AccountCode}' → {(result.IsValid ? "valid" : "invalid")}.",
                module: "CoaRead"), cancellationToken);

            return new ApiResponseDTO<PostingValidationResultDto>
            {
                IsSuccess = true,
                Message = result.IsValid ? "Valid for posting." : "Not valid for posting.",
                Data = result
            };
        }
    }
}
