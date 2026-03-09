using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Commands.CreateDocumentSequence
{
    public class CreateDocumentSequenceCommandHandler : IRequestHandler<CreateDocumentSequenceCommand, ApiResponseDTO<int>>
    {
        private readonly IDocumentSequenceCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDocumentSequenceCommandHandler(
            IDocumentSequenceCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDocumentSequenceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DocumentSequence>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DOCUMENT_SEQUENCE_CREATE",
                actionName: $"Type:{request.TypeId}-Year:{request.FinancialYearId}-Doc:{request.DocNo}",
                details: $"Document Sequence created with TypeId {request.TypeId}, FinancialYearId {request.FinancialYearId}, DocNo {request.DocNo}, Id {newId}.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Document Sequence created successfully.",
                Data = newId
            };
        }
    }
}
