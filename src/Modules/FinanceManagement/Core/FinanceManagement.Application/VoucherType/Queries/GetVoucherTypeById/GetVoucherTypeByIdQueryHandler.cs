using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeById
{
    public class GetVoucherTypeByIdQueryHandler : IRequestHandler<GetVoucherTypeByIdQuery, VoucherTypeMasterDto?>
    {
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVoucherTypeByIdQueryHandler(IVoucherTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VoucherTypeMasterDto?> Handle(GetVoucherTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<VoucherTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetVoucherTypeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Voucher Type details {dto.Id} was fetched.",
                module: "VoucherType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
