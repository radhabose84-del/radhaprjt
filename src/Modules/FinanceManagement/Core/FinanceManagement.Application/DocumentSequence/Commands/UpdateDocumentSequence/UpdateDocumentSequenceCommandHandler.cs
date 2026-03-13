using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence
{
    public class UpdateDocumentSequenceCommandHandler : IRequestHandler<UpdateDocumentSequenceCommand, ApiResponseDTO<int>>
    {
        private readonly IDocumentSequenceCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDocumentSequenceCommandHandler(
            IDocumentSequenceCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateDocumentSequenceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DocumentSequence>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DOCUMENT_SEQUENCE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Document Sequence with Id {request.Id} updated successfully.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Document Sequence updated successfully.",
                Data = result
            };
        }
    }
}
