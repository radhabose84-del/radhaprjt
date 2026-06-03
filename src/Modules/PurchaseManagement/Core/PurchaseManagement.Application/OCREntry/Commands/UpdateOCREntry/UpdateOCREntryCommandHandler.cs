using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry
{
    public class UpdateOCREntryCommandHandler : IRequestHandler<UpdateOCREntryCommand, ApiResponseDTO<int>>
    {
        private readonly IOCREntryCommandRepository _commandRepository;
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateOCREntryCommandHandler(
            IOCREntryCommandRepository commandRepository,
            IOCREntryQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateOCREntryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.OCREntry>(request);

            var result = await _commandRepository.UpdateAsync(entity, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "OCR_UPDATE",
                actionName: request.Id.ToString(),
                details: $"OCR with Id {request.Id} updated successfully.",
                module: "OCREntry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "OCR updated successfully.",
                Data = result
            };
        }
    }
}
