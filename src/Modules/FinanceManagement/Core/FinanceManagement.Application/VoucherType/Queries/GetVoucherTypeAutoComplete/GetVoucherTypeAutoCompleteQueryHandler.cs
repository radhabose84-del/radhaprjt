using AutoMapper;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeAutoComplete
{
    public class GetVoucherTypeAutoCompleteQueryHandler : IRequestHandler<GetVoucherTypeAutoCompleteQuery, IReadOnlyList<VoucherTypeLookupDto>>
    {
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVoucherTypeAutoCompleteQueryHandler(IVoucherTypeMasterQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<VoucherTypeLookupDto>> Handle(GetVoucherTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId();
            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, cancellationToken);
            var dtos = _mapper.Map<List<VoucherTypeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetVoucherTypeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Voucher Type details was fetched.",
                module: "VoucherType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
