using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade
{
    public class UpdateVendorRatingGradeCommandHandler : IRequestHandler<UpdateVendorRatingGradeCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorRatingGradeCommandRepository _commandRepository;
        private readonly IVendorRatingGradeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateVendorRatingGradeCommandHandler(
            IVendorRatingGradeCommandRepository commandRepository,
            IVendorRatingGradeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateVendorRatingGradeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.VendorRatingGrade>(request);
            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VENDOR_RATING_GRADE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"VendorRatingGrade with Id {request.Id} updated successfully.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorRatingGrade updated successfully.",
                Data = result
            };
        }
    }
}
