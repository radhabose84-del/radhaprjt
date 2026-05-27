using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade
{
    public class CreateVendorRatingGradeCommandHandler : IRequestHandler<CreateVendorRatingGradeCommand, ApiResponseDTO<int>>
    {
        private readonly IVendorRatingGradeCommandRepository _commandRepository;
        private readonly IVendorRatingGradeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateVendorRatingGradeCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateVendorRatingGradeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.VendorRatingGrade>(request);
            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "VENDOR_RATING_GRADE_CREATE",
                actionName: request.GradeCode ?? string.Empty,
                details: $"VendorRatingGrade '{request.GradeCode}' created successfully with Id {newId}.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "VendorRatingGrade created successfully.",
                Data = newId
            };
        }
    }
}
