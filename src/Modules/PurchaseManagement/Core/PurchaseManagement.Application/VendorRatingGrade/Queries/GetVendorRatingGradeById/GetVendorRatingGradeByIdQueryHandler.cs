using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeById
{
    public class GetVendorRatingGradeByIdQueryHandler : IRequestHandler<GetVendorRatingGradeByIdQuery, VendorRatingGradeDto?>
    {
        private readonly IVendorRatingGradeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorRatingGradeByIdQueryHandler(IVendorRatingGradeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VendorRatingGradeDto?> Handle(GetVendorRatingGradeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null) return null;

            var dto = _mapper.Map<VendorRatingGradeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetVendorRatingGradeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"VendorRatingGrade details {dto.Id} was fetched.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
