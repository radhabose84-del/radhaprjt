using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetAllVoucherType
{
    public class GetAllVoucherTypeQueryHandler : IRequestHandler<GetAllVoucherTypeQuery, ApiResponseDTO<List<VoucherTypeMasterDto>>>
    {
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVoucherTypeQueryHandler(IVoucherTypeMasterQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VoucherTypeMasterDto>>> Handle(GetAllVoucherTypeQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId, request.FinancialYearId);

            var dtos = _mapper.Map<List<VoucherTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVoucherTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Voucher Type details were fetched.",
                module: "VoucherType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VoucherTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Voucher Type list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
